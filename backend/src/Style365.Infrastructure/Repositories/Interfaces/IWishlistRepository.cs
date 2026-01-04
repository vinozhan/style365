using Style365.Domain.Entities;

namespace Style365.Infrastructure.Repositories.Interfaces;

public interface IWishlistRepository : IRepository<Wishlist>
{
    Task<IEnumerable<Wishlist>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Wishlist?> GetDefaultWishlistByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Wishlist?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Wishlist>> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Wishlist>> GetPublicWishlistsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsProductInWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Wishlist>> GetWishlistsContainingProductAsync(Guid productId, CancellationToken cancellationToken = default);
}