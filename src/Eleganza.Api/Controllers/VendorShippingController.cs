using Eleganza.Application.Shipping;
using Eleganza.Contracts.Shipping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eleganza.Api.Controllers;

[ApiController]
[Authorize(Roles = "VendorOwner")]
[Route("api/vendors/me/shipping/vanex")]
public sealed class VendorShippingController(VendorShippingAccountService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ShippingAccountResponse>> Get(CancellationToken cancellationToken)
    {
        var account = await service.GetVanexAsync(cancellationToken);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpPut]
    public async Task<ActionResult<ShippingAccountResponse>> Connect(
        ConnectVanexRequest request,
        CancellationToken cancellationToken)
        => Ok(await service.ConnectVanexAsync(request, cancellationToken));

    [HttpDelete]
    public async Task<ActionResult<ShippingAccountResponse>> Disconnect(CancellationToken cancellationToken)
        => Ok(await service.DisconnectVanexAsync(cancellationToken));
}
