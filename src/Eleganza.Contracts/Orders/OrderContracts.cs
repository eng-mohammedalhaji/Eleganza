using Eleganza.Domain.Enums;

namespace Eleganza.Contracts.Orders;

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    Guid VariantId,
    int Quantity);

public sealed record CreateOrderRequest(
    string CustomerName,
    string CustomerPhone,
    string City,
    string Address,
    string? Notes,
    IReadOnlyList<CreateOrderItemRequest> Items);

public sealed record OrderItemResponse(
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string Size,
    string Color,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record OrderResponse(
    Guid Id,
    string OrderNumber,
    Guid VendorId,
    Guid? CustomerId,
    string CustomerName,
    string CustomerPhone,
    string City,
    string Address,
    string? Notes,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Total,
    PaymentMethod PaymentMethod,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    ShippingStatus ShippingStatus,
    string? ExternalShippingOrderId,
    IReadOnlyList<OrderItemResponse> Items,
    DateTimeOffset CreatedAt);
