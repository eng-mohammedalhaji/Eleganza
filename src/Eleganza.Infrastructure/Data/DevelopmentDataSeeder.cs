using Eleganza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eleganza.Infrastructure.Data;

public static class DevelopmentDataSeeder
{
    private static readonly Guid DemoOwnerId = Guid.Parse("3b9e4e5a-7d1e-4e9b-9a18-2d2c8a1a0001");

    private sealed record CategorySeed(string Name, string Slug, string Description);

    private sealed record ProductSeed(
        string CategorySlug,
        string Name,
        string Slug,
        string Description,
        decimal Price,
        int Stock,
        string[] Sizes,
        string[] Colors,
        string ImageUrl,
        bool Featured);

    public static async Task SeedCatalogAsync(
        AppDbContext db,
        CancellationToken cancellationToken = default)
    {
        var categorySeeds = new[]
        {
            new CategorySeed("فساتين سهرة", "evening", "تصاميم للمناسبات والسهرات."),
            new CategorySeed("فساتين زفاف", "bridal", "إطلالات العروس وتفاصيلها الراقية."),
            new CategorySeed("فساتين يومية", "daily", "فساتين عملية وأنيقة للاستخدام اليومي."),
            new CategorySeed("إطلالات محتشمة", "modest", "قصّات طويلة ومحتشمة بتفاصيل عصرية."),
        };

        foreach (var seed in categorySeeds)
        {
            if (!await db.Categories.AnyAsync(category => category.Slug == seed.Slug, cancellationToken))
            {
                db.Categories.Add(Category.Create(seed.Name, seed.Slug, seed.Description));
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var vendor = await db.Vendors.SingleOrDefaultAsync(item => item.Slug == "eleganza-demo", cancellationToken);
        if (vendor is null)
        {
            vendor = Vendor.CreateApplication(
                DemoOwnerId,
                "إيليجانزا التجريبي",
                "eleganza-demo",
                "0910000000",
                "طرابلس");
            vendor.Approve();
            db.Vendors.Add(vendor);
            await db.SaveChangesAsync(cancellationToken);
        }

        var categories = await db.Categories
            .Where(category => category.IsActive)
            .ToDictionaryAsync(category => category.Slug, cancellationToken);

        foreach (var seed in ProductSeeds)
        {
            if (await db.Products.AnyAsync(product => product.Slug == seed.Slug, cancellationToken))
            {
                continue;
            }

            var category = categories[seed.CategorySlug];
            var product = Product.CreateDraft(
                vendor.Id,
                category.Id,
                seed.Name,
                seed.Slug,
                seed.Description);

            var combinationCount = seed.Sizes.Length * seed.Colors.Length;
            var baseStock = Math.Max(1, seed.Stock / combinationCount);
            var remainder = Math.Max(0, seed.Stock - (baseStock * combinationCount));
            var variantIndex = 0;

            foreach (var color in seed.Colors)
            {
                foreach (var size in seed.Sizes)
                {
                    var stock = baseStock + (variantIndex < remainder ? 1 : 0);
                    product.AddVariant(
                        size,
                        color,
                        $"{seed.Slug.ToUpperInvariant()}-{size}-{variantIndex + 1:00}",
                        seed.Price,
                        stock);
                    variantIndex++;
                }
            }

            product.AddMedia(seed.ImageUrl, seed.Name, 0);
            product.SetFeatured(seed.Featured);
            product.SubmitForApproval();
            product.Approve();
            db.Products.Add(product);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static readonly ProductSeed[] ProductSeeds =
    [
        new("evening", "فستان روز ساتان", "rose-satin-dress", "فستان ساتان بقصة ناعمة ولمعة راقية، مصمم للمناسبات التي تحتاج حضوراً لا يُنسى.", 485, 8, ["S", "M", "L", "XL"], ["وردي غباري", "أسود"], "https://images.unsplash.com/photo-1566174053879-31528523f8ae?auto=format&fit=crop&w=1000&q=85", true),
        new("evening", "مخمل منتصف الليل", "midnight-velvet", "تصميم مخملي بكتف واحد وتفاصيل منحوتة تمنح الإطلالة فخامة هادئة.", 620, 5, ["S", "M", "L"], ["أسود", "أزرق ليلي"], "https://images.unsplash.com/photo-1595777457583-95e059d581b8?auto=format&fit=crop&w=1000&q=85", true),
        new("bridal", "حديقة اللؤلؤ", "pearl-garden", "فستان أبيض بتطريزات لؤلؤية خفيفة وتفاصيل رومانسية لعروس عصرية.", 1450, 3, ["S", "M", "L", "XL"], ["أبيض عاجي"], "https://images.unsplash.com/photo-1519741497674-611481863552?auto=format&fit=crop&w=1000&q=85", true),
        new("modest", "دريب الزيتون", "olive-drape", "قصة طويلة بانسدال أنيق وأكمام واسعة، مناسبة للإطلالات المحتشمة الراقية.", 390, 11, ["M", "L", "XL", "XXL"], ["زيتوني", "بيج"], "https://images.unsplash.com/photo-1539008835657-9e8e9680c956?auto=format&fit=crop&w=1000&q=85", true),
        new("evening", "عمود الشمبانيا", "champagne-column", "فستان بقصة مستقيمة ولمسة لونية دافئة، خيار مثالي لحفلات العشاء والمناسبات.", 545, 6, ["S", "M", "L"], ["شمبانيا", "بني داكن"], "https://images.unsplash.com/photo-1496747611176-843222e1e57c?auto=format&fit=crop&w=1000&q=85", false),
        new("daily", "نسمة كتان", "linen-summer", "فستان كتان خفيف بقصة مريحة وألوان مستوحاة من صباحات الصيف الهادئة.", 280, 14, ["S", "M", "L", "XL"], ["أبيض", "أزرق سماوي"], "https://images.unsplash.com/photo-1496217590455-aa63a8350eea?auto=format&fit=crop&w=1000&q=85", false),
        new("evening", "طيات الياقوت", "ruby-pleats", "طيات انسيابية ولون ياقوتي غني يرفع أي إطلالة مسائية إلى مستوى آخر.", 510, 4, ["S", "M", "L", "XL"], ["ياقوتي", "بنفسجي"], "https://images.unsplash.com/photo-1485968579580-b6d095142e6e?auto=format&fit=crop&w=1000&q=85", false),
        new("daily", "ميدي الرمال", "sand-midi", "تصميم ميدي عملي بأزرار أمامية وحزام ناعم، للّوك اليومي الأنيق.", 315, 9, ["S", "M", "L"], ["رملي", "أسود"], "https://images.unsplash.com/photo-1525507119028-ed4c629a60a3?auto=format&fit=crop&w=1000&q=85", false),
    ];
}
