namespace Eleganza.Domain.Entities;

public sealed class OrderItem
{
    private OrderItem()
    {
    }

    private OrderItem(
        Guid orderId,
        Guid productId,
        Guid productVariantId,
        string productName,
        string size,
        string color,
        int quantity,
        decimal unitPrice)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        ProductName = productName;
        Size = size;
        Color = color;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = unitPrice * quantity;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string Size { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    public static OrderItem Create(
        Guid orderId,
        Guid productId,
        Guid productVariantId,
        string productName,
        string size,
        string color,
        int quantity,
        decimal unitPrice)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        if (unitPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice));
        }

        return new OrderItem(orderId, productId, productVariantId, productName, size, color, quantity, unitPrice);
    }
}
