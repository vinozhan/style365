using Microsoft.EntityFrameworkCore;
using Style365.Domain.Entities;
using Style365.Infrastructure.Data;
using Style365.Infrastructure.Repositories.Interfaces;

namespace Style365.Infrastructure.Repositories;

public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
{
    public ShoppingCartRepository(Style365DbContext context) : base(context)
    {
    }

    public async Task<ShoppingCart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(sc => sc.UserId == userId, cancellationToken);
    }

    public async Task<ShoppingCart?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(sc => sc.SessionId == sessionId, cancellationToken);
    }

    public async Task<ShoppingCart?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(sc => sc.Items)
            .ThenInclude(ci => ci.Product)
            .Include(sc => sc.Items)
            .ThenInclude(ci => ci.ProductVariant)
            .FirstOrDefaultAsync(sc => sc.Id == id, cancellationToken);
    }

    public async Task<ShoppingCart?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(sc => sc.Items)
            .ThenInclude(ci => ci.Product)
            .Include(sc => sc.Items)
            .ThenInclude(ci => ci.ProductVariant)
            .FirstOrDefaultAsync(sc => sc.UserId == userId, cancellationToken);
    }

    public async Task<ShoppingCart?> GetBySessionIdWithItemsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(sc => sc.Items)
            .ThenInclude(ci => ci.Product)
            .Include(sc => sc.Items)
            .ThenInclude(ci => ci.ProductVariant)
            .FirstOrDefaultAsync(sc => sc.SessionId == sessionId, cancellationToken);
    }

    public async Task<IEnumerable<ShoppingCart>> GetExpiredCartsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(sc => sc.ExpiresAt.HasValue && sc.ExpiresAt < now)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ShoppingCart>> GetAbandonedCartsAsync(int daysSinceLastModified, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysSinceLastModified);
        return await _dbSet
            .Where(sc => sc.LastModified < cutoffDate)
            .Include(sc => sc.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task CleanupExpiredCartsAsync(CancellationToken cancellationToken = default)
    {
        var expiredCarts = await GetExpiredCartsAsync(cancellationToken);
        RemoveRange(expiredCarts);
    }
}