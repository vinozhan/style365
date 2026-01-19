using Style365.Domain.Entities;

namespace Style365.Application.Common.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdWithVariantsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetProductsByBrandAsync(string brand, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default);
    Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default);
    
    // Advanced filtering
    Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsWithFiltersAsync(
        int page,
        int pageSize,
        Guid? categoryId = null,
        string? brand = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStock = null,
        bool? featuredOnly = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default);
}