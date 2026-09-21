using Eleganza.Application.Catalog;
using Eleganza.Contracts.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eleganza.Api.Controllers;

[ApiController]
[Authorize(Roles = "VendorOwner")]
[Route("api/vendor/products")]
public sealed class VendorProductsController(ProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> List(CancellationToken cancellationToken)
        => Ok(await productService.ListMineAsync(cancellationToken));
}
