using Microsoft.EntityFrameworkCore;
using Style365.Domain.Entities;
using Style365.Infrastructure.Data;
using Style365.Infrastructure.Repositories.Interfaces;

namespace Style365.Infrastructure.Repositories;

public class WishlistRepository : Repository<Wishlist>, IWishlistRepository
{
    public WishlistRepository(Style365DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Wishlist>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Wishlist?> GetDefaultWishlistByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(w => w.UserId == userId && w.IsDefault, cancellationToken);
    }

    public async Task<Wishlist?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Items)
            .ThenInclude(wi => wi.Product)
            .ThenInclude(p => p.Images.Where(i => i.IsMain))
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Wishlist>> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Items)
            .ThenInclude(wi => wi.Product)
            .ThenInclude(p => p.Images.Where(i => i.IsMain))
            .Where(w => w.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Wishlist>> GetPublicWishlistsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.IsPublic)
            .Include(w => w.User)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsProductInWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(w => w.UserId == userId && w.Items.Any(wi => wi.ProductId == productId), cancellationToken);
    }

    public async Task<IEnumerable<Wishlist>> GetWishlistsContainingProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.Items.Any(wi => wi.ProductId == productId))
            .Include(w => w.User)
            .ToListAsync(cancellationToken);
    }
}