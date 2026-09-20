using Eleganza.Application.Catalog;
using Eleganza.Contracts.Catalog;
using Eleganza.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eleganza.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ProductService productService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> List(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
        => Ok(await productService.ListPublishedAsync(categoryId, search, cancellationToken));

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductResponse>> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var product = await productService.GetPublishedBySlugAsync(slug, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "VendorOwner")]
    public async Task<ActionResult<ProductResponse>> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await productService.CreateAsync(request, cancellationToken));

    [HttpPost("{id:guid}/variants")]
    [Authorize(Roles = "VendorOwner")]
    public async Task<ActionResult<ProductResponse>> AddVariant(
        Guid id,
        AddProductVariantRequest request,
        CancellationToken cancellationToken)
        => Ok(await productService.AddVariantAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/media")]
    [Authorize(Roles = "VendorOwner")]
    public async Task<ActionResult<ProductResponse>> AddMedia(
        Guid id,
        AddProductMediaRequest request,
        CancellationToken cancellationToken)
        => Ok(await productService.AddMediaAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = "VendorOwner")]
    public async Task<ActionResult<ProductResponse>> Submit(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await productService.SubmitAsync(id, cancellationToken));
}
