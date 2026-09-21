using Eleganza.Domain.Entities;

namespace Eleganza.Application.Abstractions;

public sealed record ShippingCredentialsValidationResult(bool IsValid, string? Error);

public sealed record ShippingOrderItem(
    string ProductName,
    string Size,
    string Color,
    int Quantity,
    decimal UnitPrice);

public sealed record ShippingOrderRequest(
    string OrderNumber,
    string CustomerName,
    string CustomerPhone,
    string City,
    int? DeliveryCityId,
    int? DeliverySubCityId,
    string Address,
    string? MapUrl,
    string? Notes,
    decimal CashOnDeliveryAmount,
    IReadOnlyList<ShippingOrderItem> Items);

public sealed record ShippingOrderResult(
    bool Succeeded,
    string? ExternalOrderId,
    string? TrackingNumber,
    string? Error);

public sealed record ShippingLocation(string Id, string Name);

public sealed record ShippingLocationsResult(
    bool Succeeded,
    IReadOnlyList<ShippingLocation> Locations,
    string? Error);

public interface IShippingProvider
{
    string ProviderName { get; }

    Task<ShippingCredentialsValidationResult> ValidateCredentialsAsync(
        VendorShippingAccount account,
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<ShippingOrderResult> CreateOrderAsync(
        VendorShippingAccount account,
        string accessToken,
        ShippingOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<ShippingLocationsResult> GetCitiesAsync(
        VendorShippingAccount account,
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<ShippingLocationsResult> GetSubCitiesAsync(
        VendorShippingAccount account,
        string accessToken,
        string cityId,
        CancellationToken cancellationToken = default);
}

public interface ISecretProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedValue);
}

public interface IVendorShippingAccountRepository
{
    Task AddAsync(VendorShippingAccount account, CancellationToken cancellationToken = default);
    Task<VendorShippingAccount?> GetAsync(Guid vendorId, string provider, CancellationToken cancellationToken = default);
}
