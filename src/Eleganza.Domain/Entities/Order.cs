using Eleganza.Domain.Enums;

namespace Eleganza.Domain.Entities;

public sealed class Order
{
    private Order()
    {
    }

    private Order(
        string orderNumber,
        Guid vendorId,
        Guid? customerId,
        string customerName,
        string customerPhone,
        string city,
        string address,
        string? notes,
        decimal subtotal,
        decimal shippingFee,
        PaymentMethod paymentMethod)
    {
        Id = Guid.NewGuid();
        OrderNumber = orderNumber;
        VendorId = vendorId;
        CustomerId = customerId;
        CustomerName = customerName;
        CustomerPhone = customerPhone;
        City = city;
        Address = address;
        Notes = notes;
        Subtotal = subtotal;
        ShippingFee = shippingFee;
        Total = subtotal + shippingFee;
        PaymentMethod = paymentMethod;
        Status = OrderStatus.Pending;
        PaymentStatus = PaymentStatus.CashOnDeliveryPending;
        ShippingStatus = ShippingStatus.NotSubmitted;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid VendorId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal ShippingFee { get; private set; }
    public decimal Total { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public ShippingStatus ShippingStatus { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? ExternalShippingOrderId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public List<OrderItem> Items { get; private set; } = [];

    public static Order Create(
        string orderNumber,
        Guid vendorId,
        Guid? customerId,
        string customerName,
        string customerPhone,
        string city,
        string address,
        string? notes,
        decimal subtotal,
        decimal shippingFee,
        PaymentMethod paymentMethod)
    {
        if (subtotal <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(subtotal));
        }

        if (shippingFee < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(shippingFee));
        }

        return new Order(orderNumber, vendorId, customerId, customerName, customerPhone, city, address,
            notes, subtotal, shippingFee, paymentMethod);
    }

    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Items cannot be added after order creation.");
        }

        Items.Add(item);
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be confirmed.");
        }

        Status = OrderStatus.Confirmed;
        Touch();
    }

    public void Reject(string? reason)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be rejected.");
        }

        Status = OrderStatus.Rejected;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        Touch();
    }

    public void Cancel(string? reason)
    {
        if (Status is not OrderStatus.Pending and not OrderStatus.Confirmed)
        {
            throw new InvalidOperationException("Only pending or confirmed orders can be cancelled.");
        }

        Status = OrderStatus.Cancelled;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        Touch();
    }

    public void SetShippingStatus(ShippingStatus status)
    {
        ShippingStatus = status;
        Touch();
    }

    public void SetExternalShippingOrderId(string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("External shipping order id is required.", nameof(externalId));
        }

        ExternalShippingOrderId = externalId.Trim();
        Touch();
    }

    public void MarkCollected()
    {
        if (PaymentMethod != PaymentMethod.CashOnDelivery || ShippingStatus != ShippingStatus.Delivered)
        {
            throw new InvalidOperationException("Cash can only be collected after delivery.");
        }

        PaymentStatus = PaymentStatus.Collected;
        Status = OrderStatus.Completed;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
