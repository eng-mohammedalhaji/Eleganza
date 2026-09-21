using Eleganza.Application.Abstractions;
using Eleganza.Contracts.Shipping;
using Eleganza.Domain.Enums;

namespace Eleganza.Application.Shipping;

public sealed class VanexLocationService(
    IVendorRepository vendors,
    IVendorShippingAccountRepository accounts,
    IShippingProvider shippingProvider,
    ISecretProtector secretProtector)
{
    public Task<IReadOnlyList<ShippingLocationResponse>> GetCitiesAsync(
        Guid vendorId,
        CancellationToken cancellationToken = default)
        => GetLocationsAsync(vendorId, (account, token) =>
            shippingProvider.GetCitiesAsync(account, token, cancellationToken), cancellationToken);

    public Task<IReadOnlyList<ShippingLocationResponse>> GetSubCitiesAsync(
        Guid vendorId,
        string cityId,
        CancellationToken cancellationToken = default)
        => GetLocationsAsync(vendorId, (account, token) =>
            shippingProvider.GetSubCitiesAsync(account, token, cityId, cancellationToken), cancellationToken);

    private async Task<IReadOnlyList<ShippingLocationResponse>> GetLocationsAsync(
        Guid vendorId,
        Func<Domain.Entities.VendorShippingAccount, string, Task<ShippingLocationsResult>> load,
        CancellationToken cancellationToken)
    {
        var vendor = await vendors.GetByIdAsync(vendorId, cancellationToken)
            ?? throw new KeyNotFoundException("Vendor profile was not found.");
        if (vendor.Status != VendorStatus.Approved)
        {
            throw new InvalidOperationException("The vendor is not currently active.");
        }

        var account = await accounts.GetAsync(vendorId, shippingProvider.ProviderName, cancellationToken)
            ?? throw new InvalidOperationException("The vendor has not connected Vanex.");
        if (account.Status != ShippingAccountStatus.Connected)
        {
            throw new InvalidOperationException("The vendor Vanex account is not connected.");
        }

        string token;
        try
        {
            token = secretProtector.Unprotect(account.EncryptedAccessToken);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("The vendor Vanex credentials could not be read.", exception);
        }

        var result = await load(account, token);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(result.Error ?? "Vanex could not load delivery locations.");
        }

        return result.Locations
            .Select(location => new ShippingLocationResponse(location.Id, location.Name))
            .ToArray();
    }
}
