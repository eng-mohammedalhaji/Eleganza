using Eleganza.Domain.Entities;
using Eleganza.Domain.Enums;

namespace Eleganza.Application.Abstractions;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> ListByVendorAsync(Guid vendorId, OrderStatus? status, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> ListAsync(OrderStatus? status, int take, CancellationToken cancellationToken = default);
}

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> ListDueAsync(
        DateTimeOffset now,
        int take,
        CancellationToken cancellationToken = default);
}

public interface IShippingFeeCalculator
{
    decimal Calculate(string city, Guid vendorId);
}
