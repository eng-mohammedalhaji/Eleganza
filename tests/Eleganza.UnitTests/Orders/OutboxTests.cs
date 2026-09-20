using Eleganza.Domain.Entities;

namespace Eleganza.UnitTests.Orders;

public sealed class OutboxTests
{
    [Fact]
    public void Outbox_message_is_due_until_processed()
    {
        var message = OutboxMessage.Create(OutboxMessageTypes.SubmitShippingOrder, "{\"orderId\":\"test\"}");

        Assert.True(message.IsDue(DateTimeOffset.UtcNow));

        message.ScheduleRetry("temporary failure", DateTimeOffset.UtcNow.AddMinutes(1));
        Assert.False(message.IsDue(DateTimeOffset.UtcNow));

        message.MarkProcessed();
        Assert.False(message.IsDue(DateTimeOffset.UtcNow));
        Assert.Equal(1, message.AttemptCount);
    }

    [Fact]
    public void Outbox_message_can_be_moved_to_dead_letter()
    {
        var message = OutboxMessage.Create(OutboxMessageTypes.SubmitShippingOrder, "{}");

        message.MoveToDeadLetter("invalid credentials");

        Assert.False(message.IsDue(DateTimeOffset.UtcNow));
        Assert.Equal("invalid credentials", message.LastError);
        Assert.NotNull(message.DeadLetteredAt);
    }
}
