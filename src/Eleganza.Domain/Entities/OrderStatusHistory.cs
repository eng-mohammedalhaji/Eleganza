using Eleganza.Domain.Enums;

namespace Eleganza.Domain.Entities;

public sealed class OrderStatusHistory
{
    private OrderStatusHistory()
    {
    }

    private OrderStatusHistory(
        Guid orderId,
        OrderStatus? fromStatus,
        OrderStatus toStatus,
        Guid? actorUserId,
        string? reason)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ActorUserId = actorUserId;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public OrderStatus? FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public string? Reason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static OrderStatusHistory Create(
        Guid orderId,
        OrderStatus? fromStatus,
        OrderStatus toStatus,
        Guid? actorUserId,
        string? reason = null)
        => new(orderId, fromStatus, toStatus, actorUserId, reason);
}
