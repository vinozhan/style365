using Style365.Domain.Entities;

namespace Style365.Application.Common.Interfaces;

public interface IShoppingCartRepository : IRepository<ShoppingCart>
{
    Task<ShoppingCart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> GetBySessionIdWithItemsAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShoppingCart>> GetExpiredCartsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ShoppingCart>> GetAbandonedCartsAsync(int daysSinceLastModified, CancellationToken cancellationToken = default);
    Task CleanupExpiredCartsAsync(CancellationToken cancellationToken = default);
}