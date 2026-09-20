namespace Eleganza.Domain.Entities;

public sealed class ProductMedia
{
    private ProductMedia()
    {
    }

    private ProductMedia(Guid productId, string storageKey, string? altText, int sortOrder)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        StorageKey = storageKey;
        AltText = altText;
        SortOrder = sortOrder;
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string? AltText { get; private set; }
    public int SortOrder { get; private set; }

    public static ProductMedia Create(Guid productId, string storageKey, string? altText, int sortOrder)
        => new(productId, storageKey, altText, sortOrder);
}
