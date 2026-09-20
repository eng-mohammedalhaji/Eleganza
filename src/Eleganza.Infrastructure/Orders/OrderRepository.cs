using Eleganza.Application.Abstractions;
using Eleganza.Domain.Entities;
using Eleganza.Domain.Enums;
using Eleganza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eleganza.Infrastructure.Orders;

public sealed class OrderRepository(AppDbContext db) : IOrderRepository
{
    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => db.Orders.AddAsync(order, cancellationToken).AsTask();

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Orders.Include(order => order.Items).Include(order => order.StatusHistory)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);

    public Task<Order?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken = default)
        => db.Orders.AsNoTracking().Include(order => order.Items)
            .Include(order => order.StatusHistory)
            .SingleOrDefaultAsync(order => order.IdempotencyKey == key, cancellationToken);

    public async Task<IReadOnlyList<Order>> ListByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
        => await db.Orders.AsNoTracking().Include(order => order.Items).Include(order => order.StatusHistory)
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> ListByVendorAsync(
        Guid vendorId,
        OrderStatus? status,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = QueryWithDetails().Where(order => order.VendorId == vendorId);
        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        return await query.OrderByDescending(order => order.CreatedAt)
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> ListAsync(
        OrderStatus? status,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = QueryWithDetails();
        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        return await query.OrderByDescending(order => order.CreatedAt)
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Order> QueryWithDetails()
        => db.Orders.AsNoTracking()
            .Include(order => order.Items)
            .Include(order => order.StatusHistory);
}

public sealed class OutboxRepository(AppDbContext db) : IOutboxRepository
{
    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        => db.OutboxMessages.AddAsync(message, cancellationToken).AsTask();

    public async Task<IReadOnlyList<OutboxMessage>> ListDueAsync(
        DateTimeOffset now,
        int take,
        CancellationToken cancellationToken = default)
        => await db.OutboxMessages
            .Where(message => message.ProcessedAt == null
                && message.DeadLetteredAt == null
                && (message.NextAttemptAt == null || message.NextAttemptAt <= now))
            .OrderBy(message => message.OccurredAt)
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync(cancellationToken);
}
