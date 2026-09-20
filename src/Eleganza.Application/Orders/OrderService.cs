using Eleganza.Application.Abstractions;
using Eleganza.Contracts.Orders;
using Eleganza.Domain.Entities;
using Eleganza.Domain.Enums;

namespace Eleganza.Application.Orders;

public sealed class OrderService(
    IOrderRepository orders,
    IProductRepository products,
    IVendorRepository vendors,
    IShippingFeeCalculator shippingFees,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
{
    public async Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Items is null || request.Items.Count is < 1 or > 50)
        {
            throw new ArgumentException("An order must contain between 1 and 50 items.", nameof(request.Items));
        }

        var customerName = RequireText(request.CustomerName, nameof(request.CustomerName), 2, 120);
        var customerPhone = RequireText(request.CustomerPhone, nameof(request.CustomerPhone), 5, 30);
        var city = RequireText(request.City, nameof(request.City), 2, 80);
        var address = RequireText(request.Address, nameof(request.Address), 5, 300);

        var duplicateVariants = request.Items.GroupBy(item => item.VariantId).FirstOrDefault(group => group.Count() > 1);
        if (duplicateVariants is not null)
        {
            throw new InvalidOperationException("The same product variant cannot appear more than once in an order.");
        }

        Guid? vendorId = null;
        decimal subtotal = 0;
        var preparedItems = new List<(Product Product, ProductVariant Variant, CreateOrderItemRequest Request)>();

        foreach (var requestItem in request.Items)
        {
            if (requestItem.Quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requestItem.Quantity));
            }

            var product = await products.GetByIdAsync(requestItem.ProductId, cancellationToken)
                ?? throw new KeyNotFoundException("One of the selected products was not found.");
            if (product.Status != ProductStatus.Published)
            {
                throw new InvalidOperationException("One of the selected products is not currently available.");
            }

            var vendor = await vendors.GetByIdAsync(product.VendorId, cancellationToken)
                ?? throw new KeyNotFoundException("The product vendor was not found.");
            if (vendor.Status != VendorStatus.Approved)
            {
                throw new InvalidOperationException("The selected vendor is not currently active.");
            }

            if (vendorId.HasValue && vendorId.Value != product.VendorId)
            {
                throw new InvalidOperationException("A checkout can contain products from one vendor only.");
            }

            var variant = product.Variants.SingleOrDefault(item => item.Id == requestItem.VariantId)
                ?? throw new KeyNotFoundException("One of the selected product variants was not found.");

            variant.Reserve(requestItem.Quantity);
            subtotal += variant.Price * requestItem.Quantity;
            vendorId = product.VendorId;
            preparedItems.Add((product, variant, requestItem));
        }

        var shippingFee = shippingFees.Calculate(city, vendorId!.Value);
        var order = Order.Create(
            GenerateOrderNumber(),
            vendorId.Value,
            currentUser.UserId,
            customerName,
            customerPhone,
            city,
            address,
            string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            subtotal,
            shippingFee,
            PaymentMethod.CashOnDelivery);

        foreach (var prepared in preparedItems)
        {
            order.AddItem(OrderItem.Create(
                order.Id,
                prepared.Product.Id,
                prepared.Variant.Id,
                prepared.Product.Name,
                prepared.Variant.Size,
                prepared.Variant.Color,
                prepared.Request.Quantity,
                prepared.Variant.Price));
        }

        await orders.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    public async Task<IReadOnlyList<OrderResponse>> ListMineAsync(CancellationToken cancellationToken = default)
    {
        var customerId = currentUser.UserId ?? throw new UnauthorizedAccessException("Authentication is required.");
        return (await orders.ListByCustomerAsync(customerId, cancellationToken)).Select(Map).ToArray();
    }

    public async Task<OrderResponse> ConfirmAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetRequiredAsync(orderId, cancellationToken);
        await RequireVendorAccessAsync(order.VendorId, cancellationToken);
        order.Confirm();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    public async Task<OrderResponse> RejectAsync(
        Guid orderId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var order = await GetRequiredAsync(orderId, cancellationToken);
        await RequireVendorAccessAsync(order.VendorId, cancellationToken);
        await ReleaseReservationsAsync(order, cancellationToken);
        order.Reject(reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    public async Task<OrderResponse> CancelAsync(
        Guid orderId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var order = await GetRequiredAsync(orderId, cancellationToken);
        var isAdmin = currentUser.IsInRole("Admin");
        var isCustomer = currentUser.UserId.HasValue && order.CustomerId == currentUser.UserId.Value;
        var isVendor = await IsVendorOwnerAsync(order.VendorId, cancellationToken);
        if (!isAdmin && !isCustomer && !isVendor)
        {
            throw new UnauthorizedAccessException("You cannot cancel this order.");
        }

        await ReleaseReservationsAsync(order, cancellationToken);
        order.Cancel(reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    public async Task<OrderResponse> MarkDeliveredAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetRequiredAsync(orderId, cancellationToken);
        await RequireVendorAccessAsync(order.VendorId, cancellationToken);
        if (order.Status != OrderStatus.Confirmed)
        {
            throw new InvalidOperationException("Only confirmed orders can be marked as delivered.");
        }

        await CommitReservationsAsync(order, cancellationToken);
        order.SetShippingStatus(ShippingStatus.Delivered);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    public async Task<OrderResponse> MarkCollectedAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetRequiredAsync(orderId, cancellationToken);
        await RequireVendorAccessAsync(order.VendorId, cancellationToken);
        order.MarkCollected();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    private async Task<Order> GetRequiredAsync(Guid orderId, CancellationToken cancellationToken)
        => await orders.GetByIdAsync(orderId, cancellationToken)
           ?? throw new KeyNotFoundException("Order was not found.");

    private async Task RequireVendorAccessAsync(Guid vendorId, CancellationToken cancellationToken)
    {
        if (currentUser.IsInRole("Admin"))
        {
            return;
        }

        if (!await IsVendorOwnerAsync(vendorId, cancellationToken))
        {
            throw new UnauthorizedAccessException("You do not own this vendor order.");
        }
    }

    private async Task<bool> IsVendorOwnerAsync(Guid vendorId, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        if (!userId.HasValue)
        {
            return false;
        }

        var vendor = await vendors.GetByIdAsync(vendorId, cancellationToken);
        return vendor?.OwnerId == userId.Value;
    }

    private async Task ReleaseReservationsAsync(Order order, CancellationToken cancellationToken)
    {
        if (order.Status is not OrderStatus.Pending and not OrderStatus.Confirmed)
        {
            return;
        }

        foreach (var item in order.Items)
        {
            var product = await products.GetByIdAsync(item.ProductId, cancellationToken)
                ?? throw new KeyNotFoundException("A product for this order was not found.");
            var variant = product.Variants.Single(itemVariant => itemVariant.Id == item.ProductVariantId);
            variant.ReleaseReservation(item.Quantity);
        }
    }

    private async Task CommitReservationsAsync(Order order, CancellationToken cancellationToken)
    {
        foreach (var item in order.Items)
        {
            var product = await products.GetByIdAsync(item.ProductId, cancellationToken)
                ?? throw new KeyNotFoundException("A product for this order was not found.");
            var variant = product.Variants.Single(itemVariant => itemVariant.Id == item.ProductVariantId);
            variant.CommitSale(item.Quantity);
        }
    }

    private static string GenerateOrderNumber()
    {
        var token = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"EL-{DateTimeOffset.UtcNow:yyyyMMdd}-{token}";
    }

    private static string RequireText(string? value, string field, int minLength, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length < minLength || normalized.Length > maxLength)
        {
            throw new ArgumentException($"{field} must be between {minLength} and {maxLength} characters.", field);
        }

        return normalized;
    }

    private static OrderResponse Map(Order order)
        => new(
            order.Id,
            order.OrderNumber,
            order.VendorId,
            order.CustomerId,
            order.CustomerName,
            order.CustomerPhone,
            order.City,
            order.Address,
            order.Notes,
            order.Subtotal,
            order.ShippingFee,
            order.Total,
            order.PaymentMethod,
            order.Status,
            order.PaymentStatus,
            order.ShippingStatus,
            order.ExternalShippingOrderId,
            order.Items.Select(item => new OrderItemResponse(
                item.ProductId,
                item.ProductVariantId,
                item.ProductName,
                item.Size,
                item.Color,
                item.Quantity,
                item.UnitPrice,
                item.LineTotal)).ToArray(),
            order.CreatedAt);
}
