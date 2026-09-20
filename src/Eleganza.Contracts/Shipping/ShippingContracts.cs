using Eleganza.Domain.Enums;

namespace Eleganza.Contracts.Shipping;

public sealed record ConnectVanexRequest(
    string AccessToken,
    string? MerchantId,
    string? BaseUrl);

public sealed record ShippingAccountResponse(
    string Provider,
    ShippingAccountStatus Status,
    string? MerchantId,
    string? BaseUrl,
    DateTimeOffset? LastValidatedAt,
    string? LastError,
    DateTimeOffset UpdatedAt);
