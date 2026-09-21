using Eleganza.Application.Abstractions;
using Eleganza.Contracts.Shipping;
using Eleganza.Domain.Entities;
using Eleganza.Domain.Enums;

namespace Eleganza.Application.Shipping;

public sealed class VendorShippingAccountService(
    IVendorShippingAccountRepository accounts,
    IVendorRepository vendors,
    IShippingProvider shippingProvider,
    ISecretProtector secretProtector,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
{
    public async Task<ShippingAccountResponse> ConnectVanexAsync(
        ConnectVanexRequest request,
        CancellationToken cancellationToken = default)
    {
        var ownerId = currentUser.UserId ?? throw new UnauthorizedAccessException("Authentication is required.");
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            throw new ArgumentException("Vanex access token is required.", nameof(request.AccessToken));
        }

        var vendor = await vendors.GetByOwnerIdAsync(ownerId, cancellationToken)
            ?? throw new InvalidOperationException("Create a vendor profile before connecting shipping.");
        if (vendor.Status != VendorStatus.Approved)
        {
            throw new InvalidOperationException("Only approved vendors can connect a shipping account.");
        }

        var account = await accounts.GetAsync(vendor.Id, shippingProvider.ProviderName, cancellationToken);
        var isNew = account is null;
        account ??= VendorShippingAccount.Create(vendor.Id, shippingProvider.ProviderName);

        account.MarkPendingValidation(
            secretProtector.Protect(request.AccessToken.Trim()),
            request.MerchantId,
            null);

        var validation = await shippingProvider.ValidateCredentialsAsync(account, request.AccessToken.Trim(), cancellationToken);
        if (validation.IsValid)
        {
            account.MarkConnected();
        }
        else
        {
            account.MarkInvalid(validation.Error ?? "Vanex rejected the credentials.");
        }

        if (isNew)
        {
            await accounts.AddAsync(account, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(account);
    }

    public async Task<ShippingAccountResponse?> GetVanexAsync(CancellationToken cancellationToken = default)
    {
        var vendor = await GetOwnedVendorAsync(cancellationToken);
        var account = await accounts.GetAsync(vendor.Id, shippingProvider.ProviderName, cancellationToken);
        return account is null ? null : Map(account);
    }

    public async Task<ShippingAccountResponse> DisconnectVanexAsync(CancellationToken cancellationToken = default)
    {
        var vendor = await GetOwnedVendorAsync(cancellationToken);
        var account = await accounts.GetAsync(vendor.Id, shippingProvider.ProviderName, cancellationToken)
            ?? throw new KeyNotFoundException("No Vanex account is connected.");

        account.Disconnect();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(account);
    }

    private async Task<Domain.Entities.Vendor> GetOwnedVendorAsync(CancellationToken cancellationToken)
    {
        var ownerId = currentUser.UserId ?? throw new UnauthorizedAccessException("Authentication is required.");
        return await vendors.GetByOwnerIdAsync(ownerId, cancellationToken)
            ?? throw new KeyNotFoundException("Vendor profile was not found.");
    }

    private static ShippingAccountResponse Map(VendorShippingAccount account)
        => new(account.Provider, account.Status, account.MerchantId, account.BaseUrl,
            account.LastValidatedAt, account.LastError, account.UpdatedAt);
}
