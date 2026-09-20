using Eleganza.Domain.Enums;

namespace Eleganza.Domain.Entities;

public sealed class VendorShippingAccount
{
    private VendorShippingAccount()
    {
    }

    private VendorShippingAccount(Guid vendorId, string provider)
    {
        Id = Guid.NewGuid();
        VendorId = vendorId;
        Provider = provider;
        Status = ShippingAccountStatus.Disconnected;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public Guid VendorId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string EncryptedAccessToken { get; private set; } = string.Empty;
    public string? MerchantId { get; private set; }
    public string? BaseUrl { get; private set; }
    public ShippingAccountStatus Status { get; private set; }
    public DateTimeOffset? LastValidatedAt { get; private set; }
    public string? LastError { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static VendorShippingAccount Create(Guid vendorId, string provider)
        => new(vendorId, provider);

    public void MarkPendingValidation(
        string encryptedAccessToken,
        string? merchantId,
        string? baseUrl)
    {
        EncryptedAccessToken = encryptedAccessToken;
        MerchantId = string.IsNullOrWhiteSpace(merchantId) ? null : merchantId.Trim();
        BaseUrl = string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl.Trim().TrimEnd('/');
        Status = ShippingAccountStatus.PendingValidation;
        LastError = null;
        Touch();
    }

    public void MarkConnected()
    {
        Status = ShippingAccountStatus.Connected;
        LastValidatedAt = DateTimeOffset.UtcNow;
        LastError = null;
        Touch();
    }

    public void MarkInvalid(string error)
    {
        Status = ShippingAccountStatus.Invalid;
        LastError = string.IsNullOrWhiteSpace(error) ? "Provider rejected the credentials." : error.Trim();
        Touch();
    }

    public void Disconnect()
    {
        EncryptedAccessToken = string.Empty;
        Status = ShippingAccountStatus.Disconnected;
        LastError = null;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
