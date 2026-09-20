using Eleganza.Domain.Entities;
using Eleganza.Domain.Enums;

namespace Eleganza.UnitTests.Orders;

public sealed class OrderTests
{
    [Fact]
    public void Reservation_reduces_available_stock_without_reducing_stock_total()
    {
        var product = Product.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), "Dress", "dress", "Description");
        var variant = product.AddVariant("38", "Black", "D-38-B", 250m, 3);

        variant.Reserve(2);

        Assert.Equal(3, variant.Stock);
        Assert.Equal(2, variant.ReservedStock);
        Assert.Equal(1, variant.AvailableStock);
    }

    [Fact]
    public void Sale_commit_reduces_stock_and_releases_reservation()
    {
        var product = Product.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), "Dress", "dress", "Description");
        var variant = product.AddVariant("38", "Black", "D-38-B", 250m, 3);
        variant.Reserve(2);

        variant.CommitSale(2);

        Assert.Equal(1, variant.Stock);
        Assert.Equal(0, variant.ReservedStock);
        Assert.Equal(1, variant.AvailableStock);
    }

    [Fact]
    public void Pending_order_can_be_confirmed_but_not_cancelled_after_completion()
    {
        var order = CreateOrder();

        order.Confirm();
        order.SetShippingStatus(ShippingStatus.Delivered);
        order.MarkCollected();

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Throws<InvalidOperationException>(() => order.Cancel("Too late"));
    }

    [Fact]
    public void Cash_cannot_be_collected_before_delivery()
    {
        var order = CreateOrder();

        Assert.Throws<InvalidOperationException>(() => order.MarkCollected());
    }

    private static Order CreateOrder()
    {
        var product = Product.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), "Dress", "dress", "Description");
        var variant = product.AddVariant("38", "Black", "D-38-B", 250m, 3);
        var order = Order.Create("EL-TEST-001", product.VendorId, Guid.NewGuid(), "Sara", "0910000000", "Tripoli", "Street 1", null, 250m, 0m, PaymentMethod.CashOnDelivery);
        order.AddItem(OrderItem.Create(order.Id, product.Id, variant.Id, product.Name, variant.Size, variant.Color, 1, variant.Price));
        return order;
    }
}
