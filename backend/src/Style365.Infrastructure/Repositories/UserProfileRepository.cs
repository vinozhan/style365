using Microsoft.EntityFrameworkCore;
using Style365.Domain.Entities;
using Style365.Infrastructure.Data;
using Style365.Application.Common.Interfaces;

namespace Style365.Infrastructure.Repositories;

public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(Style365DbContext context) : base(context)
    {
    }

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);
    }

    public async Task<UserProfile?> GetByUserIdWithAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<UserProfile>> GetProfilesByPreferredCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(up => up.FavoriteCategories != null && up.FavoriteCategories.Contains(category))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserProfile>> GetProfilesByPreferredBrandAsync(string brand, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(up => up.PreferredBrands != null && up.PreferredBrands.Contains(brand))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserProfile>> GetByPreferencesAsync(string preferences, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(up => up.FavoriteCategories != null && up.FavoriteCategories.Contains(preferences))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(up => up.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<UserProfile>> GetProfilesWithAddressesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(up => up.Addresses.Any())
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetProfileCountByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(up => up.CreatedAt >= startDate && up.CreatedAt <= endDate, cancellationToken);
    }
}