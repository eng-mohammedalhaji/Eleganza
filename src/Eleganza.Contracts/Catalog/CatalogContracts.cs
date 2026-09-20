using Eleganza.Domain.Enums;

namespace Eleganza.Contracts.Catalog;

public sealed record CreateCategoryRequest(string Name, string Slug, string? Description);

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive);

public sealed record CreateProductRequest(
    Guid CategoryId,
    string Name,
    string Slug,
    string Description);

public sealed record AddProductVariantRequest(
    string Size,
    string Color,
    string Sku,
    decimal Price,
    int Stock);

public sealed record AddProductMediaRequest(
    string StorageKey,
    string? AltText,
    int SortOrder);

public sealed record ProductReviewRequest(string? Note);

public sealed record ProductVariantResponse(
    Guid Id,
    string Size,
    string Color,
    string Sku,
    decimal Price,
    int Stock);

public sealed record ProductMediaResponse(
    Guid Id,
    string StorageKey,
    string? AltText,
    int SortOrder);

public sealed record ProductResponse(
    Guid Id,
    Guid VendorId,
    Guid CategoryId,
    string VendorName,
    string CategoryName,
    string CategorySlug,
    string Name,
    string Slug,
    string Description,
    ProductStatus Status,
    string? ReviewNote,
    bool IsFeatured,
    IReadOnlyList<ProductVariantResponse> Variants,
    IReadOnlyList<ProductMediaResponse> Media);
