using Eleganza.Application.Abstractions;
using Eleganza.Contracts.Vendors;
using Eleganza.Domain.Entities;
using Eleganza.Domain.Enums;

namespace Eleganza.Application.Vendors;

public sealed class VendorService(
    IVendorRepository vendors,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IIdentityRoleService identityRoles)
{
    public async Task<VendorResponse> ApplyAsync(
        CreateVendorRequest request,
        CancellationToken cancellationToken = default)
    {
        var ownerId = RequireUserId();
        var businessName = RequireText(request.BusinessName, nameof(request.BusinessName), 2, 120);
        var slug = NormalizeSlug(request.Slug);
        var phone = RequireText(request.Phone, nameof(request.Phone), 5, 30);
        var city = RequireText(request.City, nameof(request.City), 2, 80);

        if (await vendors.GetByOwnerIdAsync(ownerId, cancellationToken) is not null)
        {
            throw new InvalidOperationException("The current user already has a vendor profile.");
        }

        if (await vendors.SlugExistsAsync(slug, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("This vendor slug is already in use.");
        }

        var vendor = Vendor.CreateApplication(ownerId, businessName, slug, phone, city);
        await vendors.AddAsync(vendor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(vendor);
    }

    public async Task<VendorResponse?> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var ownerId = RequireUserId();
        var vendor = await vendors.GetByOwnerIdAsync(ownerId, cancellationToken);
        return vendor is null ? null : Map(vendor);
    }

    public async Task<IReadOnlyList<VendorResponse>> ListAsync(
        VendorStatus? status,
        CancellationToken cancellationToken = default)
    {
        var items = await vendors.ListAsync(status, cancellationToken);
        return items.Select(Map).ToArray();
    }

    public Task<VendorResponse> StartReviewAsync(Guid vendorId, CancellationToken cancellationToken = default)
        => TransitionAsync(vendorId, vendor => vendor.StartReview(), cancellationToken);

    public async Task<VendorResponse> ApproveAsync(
        Guid vendorId,
        CancellationToken cancellationToken = default)
    {
        var vendor = await GetRequiredAsync(vendorId, cancellationToken);
        vendor.Approve();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await identityRoles.AddToRoleAsync(vendor.OwnerId, "VendorOwner", cancellationToken);
        return Map(vendor);
    }

    public Task<VendorResponse> RejectAsync(
        Guid vendorId,
        string? note,
        CancellationToken cancellationToken = default)
        => TransitionAsync(vendorId, vendor => vendor.Reject(note), cancellationToken);

    public Task<VendorResponse> SuspendAsync(
        Guid vendorId,
        string? note,
        CancellationToken cancellationToken = default)
        => TransitionAsync(vendorId, vendor => vendor.Suspend(note), cancellationToken);

    public Task<VendorResponse> ReactivateAsync(
        Guid vendorId,
        CancellationToken cancellationToken = default)
        => TransitionAsync(vendorId, vendor => vendor.Reactivate(), cancellationToken);

    private async Task<VendorResponse> TransitionAsync(
        Guid vendorId,
        Action<Vendor> transition,
        CancellationToken cancellationToken)
    {
        var vendor = await GetRequiredAsync(vendorId, cancellationToken);
        transition(vendor);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(vendor);
    }

    private async Task<Vendor> GetRequiredAsync(Guid vendorId, CancellationToken cancellationToken)
        => await vendors.GetByIdAsync(vendorId, cancellationToken)
           ?? throw new KeyNotFoundException("Vendor was not found.");

    private Guid RequireUserId()
        => currentUser.UserId ?? throw new UnauthorizedAccessException("Authentication is required.");

    private static string RequireText(string? value, string field, int minLength, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length < minLength || normalized.Length > maxLength)
        {
            throw new ArgumentException($"{field} must be between {minLength} and {maxLength} characters.", field);
        }

        return normalized;
    }

    private static string NormalizeSlug(string? value)
    {
        var slug = RequireText(value, nameof(value), 2, 80).ToLowerInvariant();
        if (slug.Any(character => !(char.IsLetterOrDigit(character) || character is '-' or '_')))
        {
            throw new ArgumentException("Slug may contain only letters, numbers, '-' or '_'.", nameof(value));
        }

        return slug;
    }

    private static VendorResponse Map(Vendor vendor)
        => new(vendor.Id, vendor.OwnerId, vendor.BusinessName, vendor.Slug, vendor.Phone, vendor.City,
            vendor.Status, vendor.ReviewNote, vendor.CreatedAt, vendor.UpdatedAt);
}
