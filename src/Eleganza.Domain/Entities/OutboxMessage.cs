namespace Eleganza.Domain.Entities;

public static class OutboxMessageTypes
{
    public const string SubmitShippingOrder = "SubmitShippingOrder";
}

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(string type, string payload)
    {
        Id = Guid.NewGuid();
        Type = type;
        Payload = payload;
        OccurredAt = DateTimeOffset.UtcNow;
        NextAttemptAt = OccurredAt;
    }

    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public DateTimeOffset? NextAttemptAt { get; private set; }
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }
    public DateTimeOffset? DeadLetteredAt { get; private set; }

    public static OutboxMessage Create(string type, string payload)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Outbox message type is required.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("Outbox message payload is required.", nameof(payload));
        }

        return new OutboxMessage(type.Trim(), payload);
    }

    public bool IsDue(DateTimeOffset now)
        => ProcessedAt is null
           && DeadLetteredAt is null
           && (NextAttemptAt is null || NextAttemptAt <= now);

    public void MarkProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
        NextAttemptAt = null;
        LastError = null;
    }

    public void ScheduleRetry(string error, DateTimeOffset nextAttemptAt)
    {
        AttemptCount++;
        LastError = string.IsNullOrWhiteSpace(error) ? "Outbox processing failed." : error.Trim()[..Math.Min(error.Trim().Length, 1000)];
        NextAttemptAt = nextAttemptAt;
    }

    public void MoveToDeadLetter(string error)
    {
        AttemptCount++;
        LastError = string.IsNullOrWhiteSpace(error) ? "Outbox message moved to dead letter." : error.Trim()[..Math.Min(error.Trim().Length, 1000)];
        DeadLetteredAt = DateTimeOffset.UtcNow;
        NextAttemptAt = null;
    }
}
