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
        int? deliveryCityId,
        int? deliverySubCityId,
        string address,
        string? mapUrl,
        string? notes,
        decimal subtotal,
        decimal shippingFee,
        PaymentMethod paymentMethod,
        string? idempotencyKey,
        string? idempotencyFingerprint)
    {
        Id = Guid.NewGuid();
        OrderNumber = orderNumber;
        VendorId = vendorId;
        CustomerId = customerId;
        CustomerName = customerName;
        CustomerPhone = customerPhone;
        City = city;
        DeliveryCityId = deliveryCityId;
        DeliverySubCityId = deliverySubCityId;
        Address = address;
        MapUrl = mapUrl;
        Notes = notes;
        Subtotal = subtotal;
        ShippingFee = shippingFee;
        Total = subtotal + shippingFee;
        PaymentMethod = paymentMethod;
        IdempotencyKey = idempotencyKey;
        IdempotencyFingerprint = idempotencyFingerprint;
        Status = OrderStatus.Pending;
        PaymentStatus = PaymentStatus.CashOnDeliveryPending;
        ShippingStatus = ShippingStatus.NotSubmitted;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
        StatusHistory.Add(OrderStatusHistory.Create(Id, null, Status, null, "Order created"));
    }

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public string? IdempotencyKey { get; private set; }
    public string? IdempotencyFingerprint { get; private set; }
    public Guid VendorId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public int? DeliveryCityId { get; private set; }
    public int? DeliverySubCityId { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public string? MapUrl { get; private set; }
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
    public string? ShippingError { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public List<OrderItem> Items { get; private set; } = [];
    public List<OrderStatusHistory> StatusHistory { get; private set; } = [];

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
        PaymentMethod paymentMethod,
        string? idempotencyKey = null,
        string? idempotencyFingerprint = null,
        int? deliveryCityId = null,
        int? deliverySubCityId = null,
        string? mapUrl = null)
    {
        if (subtotal <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(subtotal));
        }

        if (shippingFee < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(shippingFee));
        }

        return new Order(orderNumber, vendorId, customerId, customerName, customerPhone, city,
            deliveryCityId, deliverySubCityId, address, mapUrl, notes, subtotal, shippingFee,
            paymentMethod, idempotencyKey, idempotencyFingerprint);
    }

    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Items cannot be added after order creation.");
        }

        Items.Add(item);
    }

    public void Confirm(Guid? actorUserId = null)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be confirmed.");
        }

        ChangeStatus(OrderStatus.Confirmed, actorUserId);
    }

    public void Reject(string? reason, Guid? actorUserId = null)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be rejected.");
        }

        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        ChangeStatus(OrderStatus.Rejected, actorUserId, reason);
    }

    public void Cancel(string? reason, Guid? actorUserId = null)
    {
        if (Status is not OrderStatus.Pending and not OrderStatus.Confirmed)
        {
            throw new InvalidOperationException("Only pending or confirmed orders can be cancelled.");
        }

        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        ChangeStatus(OrderStatus.Cancelled, actorUserId, reason);
    }

    public void SetShippingStatus(ShippingStatus status)
    {
        ShippingStatus = status;
        if (status != ShippingStatus.Failed)
        {
            ShippingError = null;
        }
        Touch();
    }

    public void MarkShippingFailed(string error)
    {
        ShippingStatus = ShippingStatus.Failed;
        ShippingError = string.IsNullOrWhiteSpace(error) ? "Shipping submission failed." : error.Trim()[..Math.Min(error.Trim().Length, 1000)];
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

    public void MarkCollected(Guid? actorUserId = null)
    {
        if (PaymentMethod != PaymentMethod.CashOnDelivery || ShippingStatus != ShippingStatus.Delivered)
        {
            throw new InvalidOperationException("Cash can only be collected after delivery.");
        }

        PaymentStatus = PaymentStatus.Collected;
        ChangeStatus(OrderStatus.Completed, actorUserId);
    }

    private void ChangeStatus(OrderStatus nextStatus, Guid? actorUserId, string? reason = null)
    {
        var previousStatus = Status;
        Status = nextStatus;
        StatusHistory.Add(OrderStatusHistory.Create(Id, previousStatus, nextStatus, actorUserId, reason));
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
