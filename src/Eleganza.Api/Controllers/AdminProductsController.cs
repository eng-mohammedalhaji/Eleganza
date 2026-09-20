using Eleganza.Application.Catalog;
using Eleganza.Contracts.Catalog;
using Eleganza.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eleganza.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/products")]
public sealed class AdminProductsController(ProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> List(
        [FromQuery] ProductStatus? status,
        CancellationToken cancellationToken)
        => Ok(await productService.ListAdminAsync(status, cancellationToken));

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ProductResponse>> Approve(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await productService.ApproveAsync(id, cancellationToken));

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ProductResponse>> Reject(
        Guid id,
        ProductReviewRequest request,
        CancellationToken cancellationToken)
        => Ok(await productService.RejectAsync(id, request.Note, cancellationToken));
}
