using Eleganza.Application.Abstractions;
using Eleganza.Contracts.Catalog;
using Eleganza.Domain.Entities;
using Eleganza.Domain.Enums;

namespace Eleganza.Application.Catalog;

public sealed class CategoryService(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork)
{
    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = RequireText(request.Name, nameof(request.Name), 2, 80);
        var slug = NormalizeSlug(request.Slug);
        if (await categories.SlugExistsAsync(slug, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("This category slug is already in use.");
        }

        var category = Category.Create(name, slug, request.Description?.Trim());
        await categories.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task<IReadOnlyList<CategoryResponse>> ListActiveAsync(CancellationToken cancellationToken = default)
        => (await categories.ListActiveAsync(cancellationToken)).Select(Map).ToArray();

    private static string RequireText(string? value, string field, int minLength, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length < minLength || normalized.Length > maxLength)
        {
            throw new ArgumentException($"{field} must be between {minLength} and {maxLength} characters.", field);
        }

        return normalized;
    }

    private static string NormalizeSlug(string? value)
    {
        var slug = RequireText(value, nameof(value), 2, 80).ToLowerInvariant();
        if (slug.Any(character => !(char.IsLetterOrDigit(character) || character is '-' or '_')))
        {
            throw new ArgumentException("Slug may contain only letters, numbers, '-' or '_'.", nameof(value));
        }

        return slug;
    }

    private static CategoryResponse Map(Category category)
        => new(category.Id, category.Name, category.Slug, category.Description, category.IsActive);
}

public sealed class ProductService(
    IProductRepository products,
    ICategoryRepository categories,
    IVendorRepository vendors,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
{
    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var ownerId = RequireUserId();
        var vendor = await vendors.GetByOwnerIdAsync(ownerId, cancellationToken)
            ?? throw new InvalidOperationException("Create a vendor profile before adding products.");
        if (vendor.Status != VendorStatus.Approved)
        {
            throw new InvalidOperationException("Only approved vendors can add products.");
        }

        var category = await categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException("Category was not found.");
        if (!category.IsActive)
        {
            throw new InvalidOperationException("The selected category is inactive.");
        }

        var slug = NormalizeSlug(request.Slug);
        if (await products.SlugExistsAsync(slug, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("This product slug is already in use.");
        }

        var product = Product.CreateDraft(
            vendor.Id,
            category.Id,
            RequireText(request.Name, nameof(request.Name), 2, 160),
            slug,
            RequireText(request.Description, nameof(request.Description), 2, 5000));

        await products.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(product, vendor.BusinessName, category.Name, category.Slug);
    }

    public async Task<ProductResponse> AddVariantAsync(
        Guid productId,
        AddProductVariantRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await GetOwnedProductAsync(productId, cancellationToken);
        product.AddVariant(
            RequireText(request.Size, nameof(request.Size), 1, 30),
            RequireText(request.Color, nameof(request.Color), 1, 50),
            RequireText(request.Sku, nameof(request.Sku), 2, 80),
            request.Price,
            request.Stock);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<ProductResponse> AddMediaAsync(
        Guid productId,
        AddProductMediaRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await GetOwnedProductAsync(productId, cancellationToken);
        product.AddMedia(request.StorageKey, request.AltText, request.SortOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<ProductResponse> SubmitAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await GetOwnedProductAsync(productId, cancellationToken);
        product.SubmitForApproval();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<ProductResponse> ApproveAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await GetRequiredAsync(productId, cancellationToken);
        product.Approve();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<ProductResponse> RejectAsync(
        Guid productId,
        string? note,
        CancellationToken cancellationToken = default)
    {
        var product = await GetRequiredAsync(productId, cancellationToken);
        product.Reject(note);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<IReadOnlyList<ProductResponse>> ListPublishedAsync(
        Guid? categoryId,
        string? search,
        CancellationToken cancellationToken = default)
        => (await products.ListAsync(null, categoryId, ProductStatus.Published, search, cancellationToken))
            .Select(product => Map(product)).ToArray();

    public async Task<ProductResponse?> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var product = await products.GetBySlugAsync(slug.Trim().ToLowerInvariant(), cancellationToken);
        return product is null || product.Status != ProductStatus.Published ? null : Map(product);
    }

    public async Task<IReadOnlyList<ProductResponse>> ListAdminAsync(
        ProductStatus? status,
        CancellationToken cancellationToken = default)
        => (await products.ListAsync(null, null, status, null, cancellationToken)).Select(product => Map(product)).ToArray();

    private async Task<Product> GetOwnedProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await GetRequiredAsync(productId, cancellationToken);
        var ownerId = RequireUserId();
        var vendor = await vendors.GetByIdAsync(product.VendorId, cancellationToken);
        if (vendor is null || vendor.OwnerId != ownerId)
        {
            throw new UnauthorizedAccessException("You do not own this product.");
        }

        return product;
    }

    private async Task<Product> GetRequiredAsync(Guid productId, CancellationToken cancellationToken)
        => await products.GetByIdAsync(productId, cancellationToken)
           ?? throw new KeyNotFoundException("Product was not found.");

    private Guid RequireUserId()
        => currentUser.UserId ?? throw new UnauthorizedAccessException("Authentication is required.");

    private static string RequireText(string? value, string field, int minLength, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length < minLength || normalized.Length > maxLength)
        {
            throw new ArgumentException($"{field} must be between {minLength} and {maxLength} characters.", field);
        }

        return normalized;
    }

    private static string NormalizeSlug(string? value)
    {
        var slug = RequireText(value, nameof(value), 2, 100).ToLowerInvariant();
        if (slug.Any(character => !(char.IsLetterOrDigit(character) || character is '-' or '_')))
        {
            throw new ArgumentException("Slug may contain only letters, numbers, '-' or '_'.", nameof(value));
        }

        return slug;
    }

    private static ProductResponse Map(
        Product product,
        string? vendorName = null,
        string? categoryName = null,
        string? categorySlug = null)
        => new(
            product.Id,
            product.VendorId,
            product.CategoryId,
            vendorName ?? product.Vendor?.BusinessName ?? string.Empty,
            categoryName ?? product.Category?.Name ?? string.Empty,
            categorySlug ?? product.Category?.Slug ?? string.Empty,
            product.Name,
            product.Slug,
            product.Description,
            product.Status,
            product.ReviewNote,
            product.IsFeatured,
            product.Variants.Select(variant => new ProductVariantResponse(
                variant.Id, variant.Size, variant.Color, variant.Sku, variant.Price, variant.Stock)).ToArray(),
            product.Media.Select(media => new ProductMediaResponse(
                media.Id, media.StorageKey, media.AltText, media.SortOrder)).ToArray());
}
