using Eleganza.Application.Abstractions;
using Eleganza.Domain.Entities;
using Eleganza.Domain.Enums;
using Eleganza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eleganza.Infrastructure.Repositories;

public sealed class VendorRepository(AppDbContext db) : IVendorRepository
{
    public Task AddAsync(Vendor vendor, CancellationToken cancellationToken = default)
        => db.Vendors.AddAsync(vendor, cancellationToken).AsTask();

    public Task<Vendor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Vendors.SingleOrDefaultAsync(vendor => vendor.Id == id, cancellationToken);

    public Task<Vendor?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
        => db.Vendors.SingleOrDefaultAsync(vendor => vendor.OwnerId == ownerId, cancellationToken);

    public Task<bool> SlugExistsAsync(
        string slug,
        Guid? exceptId = null,
        CancellationToken cancellationToken = default)
        => db.Vendors.AnyAsync(vendor => vendor.Slug == slug && (!exceptId.HasValue || vendor.Id != exceptId.Value), cancellationToken);

    public async Task<IReadOnlyList<Vendor>> ListAsync(
        VendorStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.Vendors.AsNoTracking().OrderByDescending(vendor => vendor.CreatedAt).AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(vendor => vendor.Status == status.Value).OrderByDescending(vendor => vendor.CreatedAt);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
