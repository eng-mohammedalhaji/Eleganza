namespace Eleganza.Domain.Entities;

public sealed class ProductVariant
{
    private ProductVariant()
    {
    }

    private ProductVariant(Guid productId, string size, string color, string sku, decimal price, int stock)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Size = size;
        Color = color;
        Sku = sku;
        Price = price;
        Stock = stock;
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string Size { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public int ReservedStock { get; private set; }
    public Guid Version { get; private set; } = Guid.NewGuid();

    public int AvailableStock => Stock - ReservedStock;

    public static ProductVariant Create(
        Guid productId,
        string size,
        string color,
        string sku,
        decimal price,
        int stock)
        => new(productId, size.Trim(), color.Trim(), sku.Trim(), price, stock);

    public void AdjustStock(int quantity)
    {
        if (Stock + quantity < ReservedStock)
        {
            throw new InvalidOperationException("Stock cannot become lower than reserved stock.");
        }

        Stock += quantity;
        Touch();
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (AvailableStock < quantity)
        {
            throw new InvalidOperationException("The requested quantity is not available.");
        }

        ReservedStock += quantity;
        Touch();
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0 || quantity > ReservedStock)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Invalid reservation quantity.");
        }

        ReservedStock -= quantity;
        Touch();
    }

    public void CommitSale(int quantity)
    {
        if (quantity <= 0 || quantity > ReservedStock || quantity > Stock)
        {
            throw new InvalidOperationException("The sale quantity is not reserved.");
        }

        Stock -= quantity;
        ReservedStock -= quantity;
        Touch();
    }

    private void Touch() => Version = Guid.NewGuid();
}
