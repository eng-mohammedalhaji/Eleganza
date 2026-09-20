using Eleganza.Application.Abstractions;
using Microsoft.AspNetCore.DataProtection;

namespace Eleganza.Infrastructure.Shipping;

public sealed class DataProtectionSecretProtector(IDataProtectionProvider provider) : ISecretProtector
{
    private readonly IDataProtector protector = provider.CreateProtector("Eleganza.VendorShippingAccount.v1");

    public string Protect(string plaintext) => protector.Protect(plaintext);

    public string Unprotect(string protectedValue) => protector.Unprotect(protectedValue);
}
