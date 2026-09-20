using Eleganza.Domain.Entities;
using Eleganza.Domain.Enums;

namespace Eleganza.Application.Abstractions;

public interface IVendorRepository
{
    Task AddAsync(Vendor vendor, CancellationToken cancellationToken = default);
    Task<Vendor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vendor?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vendor>> ListAsync(VendorStatus? status, CancellationToken cancellationToken = default);
}
