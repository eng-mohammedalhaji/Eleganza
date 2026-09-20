using Eleganza.Application.Abstractions;
using Eleganza.Domain.Entities;
using Eleganza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eleganza.Infrastructure.Repositories;

public sealed class VendorShippingAccountRepository(AppDbContext db) : IVendorShippingAccountRepository
{
    public Task AddAsync(VendorShippingAccount account, CancellationToken cancellationToken = default)
        => db.VendorShippingAccounts.AddAsync(account, cancellationToken).AsTask();

    public Task<VendorShippingAccount?> GetAsync(
        Guid vendorId,
        string provider,
        CancellationToken cancellationToken = default)
        => db.VendorShippingAccounts.SingleOrDefaultAsync(
            account => account.VendorId == vendorId && account.Provider == provider,
            cancellationToken);
}
