using Eleganza.Domain.Entities;

namespace Eleganza.Application.Abstractions;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}

public interface IShippingFeeCalculator
{
    decimal Calculate(string city, Guid vendorId);
}
