using Eleganza.Application.Abstractions;
using Eleganza.Domain.Entities;
using Eleganza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eleganza.Infrastructure.Orders;

public sealed class OrderRepository(AppDbContext db) : IOrderRepository
{
    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => db.Orders.AddAsync(order, cancellationToken).AsTask();

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Orders.Include(order => order.Items).SingleOrDefaultAsync(order => order.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Order>> ListByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
        => await db.Orders.AsNoTracking().Include(order => order.Items)
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
}
