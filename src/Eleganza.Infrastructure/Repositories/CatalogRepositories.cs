using Eleganza.Application.Abstractions;
using Eleganza.Domain.Entities;
using Eleganza.Domain.Enums;
using Eleganza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eleganza.Infrastructure.Repositories;

public sealed class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public Task AddAsync(Category category, CancellationToken cancellationToken = default)
        => db.Categories.AddAsync(category, cancellationToken).AsTask();

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Categories.SingleOrDefaultAsync(category => category.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(
        string slug,
        Guid? exceptId = null,
        CancellationToken cancellationToken = default)
        => db.Categories.AnyAsync(category => category.Slug == slug && (!exceptId.HasValue || category.Id != exceptId.Value), cancellationToken);

    public async Task<IReadOnlyList<Category>> ListActiveAsync(CancellationToken cancellationToken = default)
        => await db.Categories.AsNoTracking().Where(category => category.IsActive).OrderBy(category => category.Name).ToListAsync(cancellationToken);
}

public sealed class ProductRepository(AppDbContext db) : IProductRepository
{
    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
        => db.Products.AddAsync(product, cancellationToken).AsTask();

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Products.Include(product => product.Variants).Include(product => product.Media)
            .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(
        string slug,
        Guid? exceptId = null,
        CancellationToken cancellationToken = default)
        => db.Products.AnyAsync(product => product.Slug == slug && (!exceptId.HasValue || product.Id != exceptId.Value), cancellationToken);

    public async Task<IReadOnlyList<Product>> ListAsync(
        Guid? vendorId,
        Guid? categoryId,
        ProductStatus? status,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsNoTracking()
            .Include(product => product.Variants)
            .Include(product => product.Media)
            .AsQueryable();

        if (vendorId.HasValue)
        {
            query = query.Where(product => product.VendorId == vendorId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(product => product.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToLower();
            query = query.Where(product => product.Name.ToLower().Contains(normalized)
                || product.Description.ToLower().Contains(normalized));
        }

        return await query.OrderByDescending(product => product.CreatedAt).ToListAsync(cancellationToken);
    }
}
