using Eleganza.Application.Vendors;
using Eleganza.Contracts.Vendors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eleganza.Api.Controllers;

[ApiController]
[Route("api/vendors")]
public sealed class VendorsController(VendorService vendorService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<VendorResponse>> Apply(
        CreateVendorRequest request,
        CancellationToken cancellationToken)
    {
        var vendor = await vendorService.ApplyAsync(request, cancellationToken);
        return Created("/api/vendors/me", vendor);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<VendorResponse>> GetMine(CancellationToken cancellationToken)
    {
        var vendor = await vendorService.GetMineAsync(cancellationToken);
        return vendor is null ? NotFound() : Ok(vendor);
    }
}
