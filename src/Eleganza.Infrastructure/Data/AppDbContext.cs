using Eleganza.Domain.Entities;
using Eleganza.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Eleganza.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), Eleganza.Application.Abstractions.IUnitOfWork
{
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductMedia> ProductMedia => Set<ProductMedia>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<VendorShippingAccount> VendorShippingAccounts => Set<VendorShippingAccount>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Vendor>(entity =>
        {
            entity.ToTable("vendors");
            entity.HasKey(vendor => vendor.Id);
            entity.HasIndex(vendor => vendor.Slug).IsUnique();
            entity.HasIndex(vendor => new { vendor.OwnerId, vendor.Status });
            entity.Property(vendor => vendor.BusinessName).HasMaxLength(120).IsRequired();
            entity.Property(vendor => vendor.Slug).HasMaxLength(80).IsRequired();
            entity.Property(vendor => vendor.Phone).HasMaxLength(30).IsRequired();
            entity.Property(vendor => vendor.City).HasMaxLength(80).IsRequired();
            entity.Property(vendor => vendor.ReviewNote).HasMaxLength(500);
            entity.Property(vendor => vendor.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        });

        builder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(category => category.Id);
            entity.HasIndex(category => category.Slug).IsUnique();
            entity.Property(category => category.Name).HasMaxLength(80).IsRequired();
            entity.Property(category => category.Slug).HasMaxLength(80).IsRequired();
            entity.Property(category => category.Description).HasMaxLength(500);
        });

        builder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(product => product.Id);
            entity.HasIndex(product => product.Slug).IsUnique();
            entity.HasIndex(product => new { product.VendorId, product.Status });
            entity.HasIndex(product => new { product.CategoryId, product.Status });
            entity.Property(product => product.Name).HasMaxLength(160).IsRequired();
            entity.Property(product => product.Slug).HasMaxLength(100).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(5000).IsRequired();
            entity.Property(product => product.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.HasOne(product => product.Category).WithMany().HasForeignKey(product => product.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(product => product.Vendor).WithMany().HasForeignKey(product => product.VendorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(product => product.Variants).WithOne().HasForeignKey(variant => variant.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(product => product.Media).WithOne().HasForeignKey(media => media.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable("product_variants");
            entity.HasKey(variant => variant.Id);
            entity.HasIndex(variant => new { variant.ProductId, variant.Size, variant.Color }).IsUnique();
            entity.HasIndex(variant => variant.Sku).IsUnique();
            entity.Property(variant => variant.Size).HasMaxLength(30).IsRequired();
            entity.Property(variant => variant.Color).HasMaxLength(50).IsRequired();
            entity.Property(variant => variant.Sku).HasMaxLength(80).IsRequired();
            entity.Property(variant => variant.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(variant => variant.Version).IsConcurrencyToken().IsRequired();
        });

        builder.Entity<ProductMedia>(entity =>
        {
            entity.ToTable("product_media");
            entity.HasKey(media => media.Id);
            entity.Property(media => media.StorageKey).HasMaxLength(500).IsRequired();
            entity.Property(media => media.AltText).HasMaxLength(200);
        });

        builder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(order => order.Id);
            entity.HasIndex(order => order.OrderNumber).IsUnique();
            entity.HasIndex(order => order.IdempotencyKey).IsUnique();
            entity.HasIndex(order => new { order.CustomerId, order.CreatedAt });
            entity.HasIndex(order => new { order.VendorId, order.Status });
            entity.Property(order => order.OrderNumber).HasMaxLength(40).IsRequired();
            entity.Property(order => order.IdempotencyKey).HasMaxLength(120);
            entity.Property(order => order.IdempotencyFingerprint).HasMaxLength(64);
            entity.Property(order => order.CustomerName).HasMaxLength(120).IsRequired();
            entity.Property(order => order.CustomerPhone).HasMaxLength(30).IsRequired();
            entity.Property(order => order.City).HasMaxLength(80).IsRequired();
            entity.Property(order => order.Address).HasMaxLength(300).IsRequired();
            entity.Property(order => order.Notes).HasMaxLength(1000);
            entity.Property(order => order.Subtotal).HasPrecision(18, 2).IsRequired();
            entity.Property(order => order.ShippingFee).HasPrecision(18, 2).IsRequired();
            entity.Property(order => order.Total).HasPrecision(18, 2).IsRequired();
            entity.Property(order => order.PaymentMethod).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(order => order.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(order => order.PaymentStatus).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(order => order.ShippingStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(order => order.CancellationReason).HasMaxLength(500);
            entity.Property(order => order.ExternalShippingOrderId).HasMaxLength(120);
            entity.Property(order => order.ShippingError).HasMaxLength(1000);
            entity.HasMany(order => order.Items).WithOne().HasForeignKey(item => item.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(order => order.StatusHistory).WithOne().HasForeignKey(history => history.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ProductName).HasMaxLength(160).IsRequired();
            entity.Property(item => item.Size).HasMaxLength(30).IsRequired();
            entity.Property(item => item.Color).HasMaxLength(50).IsRequired();
            entity.Property(item => item.UnitPrice).HasPrecision(18, 2).IsRequired();
            entity.Property(item => item.LineTotal).HasPrecision(18, 2).IsRequired();
        });

        builder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("order_status_history");
            entity.HasKey(history => history.Id);
            entity.HasIndex(history => new { history.OrderId, history.CreatedAt });
            entity.Property(history => history.FromStatus).HasConversion<string>().HasMaxLength(30);
            entity.Property(history => history.ToStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(history => history.Reason).HasMaxLength(500);
        });

        builder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(message => message.Id);
            entity.HasIndex(message => new { message.ProcessedAt, message.DeadLetteredAt, message.NextAttemptAt });
            entity.Property(message => message.Type).HasMaxLength(120).IsRequired();
            entity.Property(message => message.Payload).HasColumnType("text").IsRequired();
            entity.Property(message => message.LastError).HasMaxLength(1000);
        });

        builder.Entity<VendorShippingAccount>(entity =>
        {
            entity.ToTable("vendor_shipping_accounts");
            entity.HasKey(account => account.Id);
            entity.HasIndex(account => new { account.VendorId, account.Provider }).IsUnique();
            entity.Property(account => account.Provider).HasMaxLength(40).IsRequired();
            entity.Property(account => account.EncryptedAccessToken).HasMaxLength(4000).IsRequired();
            entity.Property(account => account.MerchantId).HasMaxLength(160);
            entity.Property(account => account.BaseUrl).HasMaxLength(500);
            entity.Property(account => account.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(account => account.LastError).HasMaxLength(1000);
        });
    }
}
