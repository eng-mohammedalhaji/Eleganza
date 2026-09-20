using Eleganza.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Eleganza.Infrastructure.Orders;

public sealed class ShippingOptions
{
    public decimal DefaultFee { get; set; }
}

public sealed class FixedShippingFeeCalculator(IOptions<ShippingOptions> options) : IShippingFeeCalculator
{
    public decimal Calculate(string city, Guid vendorId) => Math.Max(0, options.Value.DefaultFee);
}
