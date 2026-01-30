using Style365.Domain.Entities;

namespace Style365.Application.Common.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetTopLevelCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdWithSubCategoriesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdWithProductsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetCategoriesByParentAsync(Guid? parentCategoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetAllWithProductCountsAsync(CancellationToken cancellationToken = default);
}