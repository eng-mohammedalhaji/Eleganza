namespace Eleganza.Api.Infrastructure;

public sealed class RateLimitOptions
{
    public int AuthPermitLimit { get; set; }
    public int AuthWindowSeconds { get; set; }
    public int VendorLocationPermitLimit { get; set; }
    public int VendorLocationWindowSeconds { get; set; }
}
