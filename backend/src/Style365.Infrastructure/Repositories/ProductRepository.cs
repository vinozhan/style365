using Microsoft.EntityFrameworkCore;
using Style365.Domain.Entities;
using Style365.Infrastructure.Data;
using Style365.Application.Common.Interfaces;

namespace Style365.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(Style365DbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<Product?> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetByIdWithVariantsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Variants.Where(v => v.IsActive))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetByIdWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Variants.Where(v => v.IsActive))
            .Include(p => p.Tags.Where(t => t.IsActive))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive && p.IsFeatured)
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsMain))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive && p.CategoryId == categoryId)
            .Include(p => p.Images.Where(i => i.IsMain))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsByBrandAsync(string brand, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive && p.Brand == brand)
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsMain))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive && p.TrackQuantity && p.StockQuantity <= p.LowStockThreshold)
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm.ToLowerInvariant();
        
        return await _dbSet
            .Where(p => p.IsActive && 
                       (p.Name.ToLower().Contains(normalizedSearchTerm) ||
                        p.Description!.ToLower().Contains(normalizedSearchTerm) ||
                        p.Sku.ToLower().Contains(normalizedSearchTerm) ||
                        p.Brand!.ToLower().Contains(normalizedSearchTerm)))
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsMain))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.Sku == sku);
        
        if (excludeProductId.HasValue)
            query = query.Where(p => p.Id != excludeProductId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.Slug == slug);
        
        if (excludeProductId.HasValue)
            query = query.Where(p => p.Id != excludeProductId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsWithFiltersAsync(
        int page,
        int pageSize,
        Guid? categoryId = null,
        string? brand = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStock = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.IsActive).AsQueryable();

        // Apply filters
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrEmpty(brand))
            query = query.Where(p => p.Brand == brand);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price.Amount >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price.Amount <= maxPrice.Value);

        if (inStock.HasValue)
        {
            if (inStock.Value)
                query = query.Where(p => !p.TrackQuantity || p.StockQuantity > 0);
            else
                query = query.Where(p => p.TrackQuantity && p.StockQuantity == 0);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var normalizedSearchTerm = searchTerm.ToLowerInvariant();
            query = query.Where(p => p.Name.ToLower().Contains(normalizedSearchTerm) ||
                                   p.Description!.ToLower().Contains(normalizedSearchTerm) ||
                                   p.Sku.ToLower().Contains(normalizedSearchTerm) ||
                                   p.Brand!.ToLower().Contains(normalizedSearchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = sortBy?.ToLowerInvariant() switch
        {
            "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            "price" => ascending ? query.OrderBy(p => p.Price.Amount) : query.OrderByDescending(p => p.Price.Amount),
            "created" => ascending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
            "updated" => ascending ? query.OrderBy(p => p.UpdatedAt) : query.OrderByDescending(p => p.UpdatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        var products = await query
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsMain))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }
}