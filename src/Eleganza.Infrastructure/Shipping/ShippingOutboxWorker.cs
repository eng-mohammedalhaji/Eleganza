using System.Text.Json;
using Eleganza.Application.Abstractions;
using Eleganza.Domain.Entities;
using Eleganza.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eleganza.Infrastructure.Shipping;

public sealed class OutboxOptions
{
    public int PollingIntervalSeconds { get; set; } = 15;
    public int BatchSize { get; set; } = 10;
    public int MaxAttempts { get; set; } = 5;
}

public sealed class ShippingOutboxWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<ShippingOutboxWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Shipping outbox batch failed.");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(Math.Max(1, options.Value.PollingIntervalSeconds)),
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var messages = await outbox.ListDueAsync(
            DateTimeOffset.UtcNow,
            Math.Clamp(options.Value.BatchSize, 1, 100),
            cancellationToken);

        foreach (var message in messages)
        {
            await ProcessMessageAsync(scope.ServiceProvider, message, cancellationToken);
        }
    }

    private async Task ProcessMessageAsync(
        IServiceProvider services,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var outbox = services.GetRequiredService<IOutboxRepository>();
        var orders = services.GetRequiredService<IOrderRepository>();
        var accounts = services.GetRequiredService<IVendorShippingAccountRepository>();
        var provider = services.GetRequiredService<IShippingProvider>();
        var secretProtector = services.GetRequiredService<ISecretProtector>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();

        if (message.Type != OutboxMessageTypes.SubmitShippingOrder
            || JsonSerializer.Deserialize<SubmitShippingOrderPayload>(message.Payload) is not { } payload)
        {
            message.MoveToDeadLetter("Unknown or invalid shipping outbox payload.");
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var order = await orders.GetByIdAsync(payload.OrderId, cancellationToken);
        if (order is null)
        {
            message.MoveToDeadLetter("The order referenced by the outbox message no longer exists.");
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        if (order.ShippingStatus is ShippingStatus.Submitted
            or ShippingStatus.Accepted
            or ShippingStatus.OutForDelivery
            or ShippingStatus.Delivered
            or ShippingStatus.Cancelled)
        {
            message.MarkProcessed();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var account = await accounts.GetAsync(order.VendorId, provider.ProviderName, cancellationToken);
        if (account is null || account.Status != ShippingAccountStatus.Connected)
        {
            await FailAsync(
                order,
                message,
                "The vendor has no connected shipping account.",
                unitOfWork,
                cancellationToken);
            return;
        }

        string accessToken;
        try
        {
            accessToken = secretProtector.Unprotect(account.EncryptedAccessToken);
        }
        catch (Exception exception)
        {
            await FailAsync(order, message, $"The vendor shipping token could not be decrypted: {exception.Message}", unitOfWork, cancellationToken);
            return;
        }

        order.SetShippingStatus(ShippingStatus.Submitting);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ShippingOrderResult result;
        try
        {
            result = await provider.CreateOrderAsync(
                account,
                accessToken,
                new ShippingOrderRequest(
                    order.OrderNumber,
                    order.CustomerName,
                    order.CustomerPhone,
                    order.City,
                    order.Address,
                    order.Notes,
                    order.Total,
                    order.Items.Select(item => new ShippingOrderItem(
                        item.ProductName,
                        item.Size,
                        item.Color,
                        item.Quantity,
                        item.UnitPrice)).ToArray()),
                cancellationToken);
        }
        catch (Exception exception)
        {
            await FailAsync(order, message, $"Shipping provider call failed: {exception.Message}", unitOfWork, cancellationToken);
            return;
        }

        if (!result.Succeeded)
        {
            await FailAsync(order, message, result.Error ?? "Shipping provider rejected the order.", unitOfWork, cancellationToken);
            return;
        }

        var externalOrderId = result.ExternalOrderId ?? result.TrackingNumber;
        if (string.IsNullOrWhiteSpace(externalOrderId))
        {
            await FailAsync(order, message, "Shipping provider returned no external order identifier.", unitOfWork, cancellationToken);
            return;
        }

        order.SetExternalShippingOrderId(externalOrderId);
        order.SetShippingStatus(ShippingStatus.Submitted);
        message.MarkProcessed();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task FailAsync(
        Domain.Entities.Order order,
        OutboxMessage message,
        string error,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        order.MarkShippingFailed(error);
        if (message.AttemptCount + 1 >= Math.Max(1, options.Value.MaxAttempts))
        {
            message.MoveToDeadLetter(error);
        }
        else
        {
            var delaySeconds = Math.Min(3600, 30 * Math.Pow(2, message.AttemptCount));
            message.ScheduleRetry(error, DateTimeOffset.UtcNow.AddSeconds(delaySeconds));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogWarning(
            "Shipping submission failed for order {OrderNumber}; attempt {AttemptCount}.",
            order.OrderNumber,
            message.AttemptCount);
    }

    private sealed record SubmitShippingOrderPayload(Guid OrderId);
}
