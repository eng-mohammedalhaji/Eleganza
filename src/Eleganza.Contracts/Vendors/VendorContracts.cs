using Eleganza.Domain.Enums;

namespace Eleganza.Contracts.Vendors;

public sealed record CreateVendorRequest(
    string BusinessName,
    string Slug,
    string Phone,
    string City);

public sealed record RejectVendorRequest(string? Note);

public sealed record SuspendVendorRequest(string? Note);

public sealed record VendorResponse(
    Guid Id,
    Guid OwnerId,
    string BusinessName,
    string Slug,
    string Phone,
    string City,
    VendorStatus Status,
    string? ReviewNote,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
