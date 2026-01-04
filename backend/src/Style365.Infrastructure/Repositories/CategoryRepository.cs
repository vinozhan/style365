using Microsoft.EntityFrameworkCore;
using Style365.Domain.Entities;
using Style365.Infrastructure.Data;
using Style365.Infrastructure.Repositories.Interfaces;

namespace Style365.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(Style365DbContext context) : base(context)
    {
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetTopLevelCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive && c.ParentCategoryId == null)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive && c.ParentCategoryId == parentCategoryId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdWithSubCategoriesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.SubCategories.Where(sc => sc.IsActive).OrderBy(sc => sc.SortOrder))
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Category?> GetByIdWithProductsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Products.Where(p => p.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.Slug == slug);
        
        if (excludeCategoryId.HasValue)
            query = query.Where(c => c.Id != excludeCategoryId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetCategoriesByParentAsync(Guid? parentCategoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive && c.ParentCategoryId == parentCategoryId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
    }
}