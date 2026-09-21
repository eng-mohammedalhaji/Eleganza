using Eleganza.Application.Shipping;
using Eleganza.Contracts.Shipping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Eleganza.Api.Controllers;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("vendor-locations")]
[Route("api/vendors/{vendorId:guid}/shipping/vanex")]
public sealed class VendorVanexLocationsController(VanexLocationService locations) : ControllerBase
{
    [HttpGet("cities")]
    public async Task<ActionResult<IReadOnlyList<ShippingLocationResponse>>> Cities(
        Guid vendorId,
        CancellationToken cancellationToken)
        => Ok(await locations.GetCitiesAsync(vendorId, cancellationToken));

    [HttpGet("cities/{cityId}/subcities")]
    public async Task<ActionResult<IReadOnlyList<ShippingLocationResponse>>> SubCities(
        Guid vendorId,
        string cityId,
        CancellationToken cancellationToken)
        => Ok(await locations.GetSubCitiesAsync(vendorId, cityId, cancellationToken));
}
