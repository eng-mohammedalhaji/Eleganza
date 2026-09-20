using Eleganza.Domain.Entities;
using Eleganza.Domain.Enums;

namespace Eleganza.UnitTests.Catalog;

public sealed class ProductTests
{
    [Fact]
    public void Product_requires_a_variant_before_submission()
    {
        var product = CreateProduct();

        Assert.Throws<InvalidOperationException>(() => product.SubmitForApproval());
        Assert.Equal(ProductStatus.Draft, product.Status);
    }

    [Fact]
    public void Product_can_be_submitted_and_approved_after_adding_variant()
    {
        var product = CreateProduct();
        product.AddVariant("38", "Black", "D-001-38-BLK", 450m, 2);

        product.SubmitForApproval();
        product.Approve();

        Assert.Equal(ProductStatus.Published, product.Status);
    }

    [Fact]
    public void Product_rejects_duplicate_size_and_color_variants()
    {
        var product = CreateProduct();
        product.AddVariant("38", "Black", "D-001-38-BLK", 450m, 2);

        Assert.Throws<InvalidOperationException>(() =>
            product.AddVariant("38", "black", "D-001-38-BLK-2", 450m, 1));
    }

    [Fact]
    public void Variant_stock_cannot_become_negative()
    {
        var product = CreateProduct();
        var variant = product.AddVariant("38", "Black", "D-001-38-BLK", 450m, 1);

        Assert.Throws<InvalidOperationException>(() => variant.AdjustStock(-2));
    }

    private static Product CreateProduct()
        => Product.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), "Evening Dress", "evening-dress", "A formal dress.");
}
