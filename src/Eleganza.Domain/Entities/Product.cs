using Eleganza.Domain.Enums;

namespace Eleganza.Domain.Entities;

public sealed class Product
{
    private Product()
    {
    }

    private Product(Guid vendorId, Guid categoryId, string name, string slug, string description)
    {
        Id = Guid.NewGuid();
        VendorId = vendorId;
        CategoryId = categoryId;
        Name = name;
        Slug = slug;
        Description = description;
        Status = ProductStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public Guid VendorId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ProductStatus Status { get; private set; }
    public string? ReviewNote { get; private set; }
    public bool IsFeatured { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Vendor? Vendor { get; private set; }
    public Category? Category { get; private set; }
    public List<ProductVariant> Variants { get; private set; } = [];
    public List<ProductMedia> Media { get; private set; } = [];

    public static Product CreateDraft(
        Guid vendorId,
        Guid categoryId,
        string name,
        string slug,
        string description)
        => new(vendorId, categoryId, name, slug, description);

    public void SubmitForApproval()
    {
        if (Status is not ProductStatus.Draft and not ProductStatus.Rejected)
        {
            throw new InvalidOperationException("Only draft or rejected products can be submitted.");
        }

        if (Variants.Count == 0)
        {
            throw new InvalidOperationException("A product must have at least one variant before approval.");
        }

        Status = ProductStatus.PendingApproval;
        ReviewNote = null;
        Touch();
    }

    public void Approve()
    {
        if (Status != ProductStatus.PendingApproval)
        {
            throw new InvalidOperationException("Only products pending approval can be approved.");
        }

        Status = ProductStatus.Published;
        ReviewNote = null;
        Touch();
    }

    public void Reject(string? note)
    {
        if (Status != ProductStatus.PendingApproval)
        {
            throw new InvalidOperationException("Only products pending approval can be rejected.");
        }

        Status = ProductStatus.Rejected;
        ReviewNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        Touch();
    }

    public void Pause()
    {
        if (Status != ProductStatus.Published)
        {
            throw new InvalidOperationException("Only published products can be paused.");
        }

        Status = ProductStatus.Paused;
        Touch();
    }

    public void Archive()
    {
        if (Status == ProductStatus.Archived)
        {
            return;
        }

        Status = ProductStatus.Archived;
        Touch();
    }

    public ProductVariant AddVariant(string size, string color, string sku, decimal price, int stock)
    {
        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be greater than zero.");
        }

        if (stock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stock), "Stock cannot be negative.");
        }

        if (Variants.Any(variant =>
                string.Equals(variant.Size, size, StringComparison.OrdinalIgnoreCase)
                && string.Equals(variant.Color, color, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("A variant with the same size and color already exists.");
        }

        var variant = ProductVariant.Create(Id, size, color, sku, price, stock);
        Variants.Add(variant);
        Touch();
        return variant;
    }

    public void AddMedia(string storageKey, string? altText, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("Storage key is required.", nameof(storageKey));
        }

        Media.Add(ProductMedia.Create(Id, storageKey.Trim(), altText?.Trim(), sortOrder));
        Touch();
    }

    public void SetFeatured(bool isFeatured)
    {
        IsFeatured = isFeatured;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
