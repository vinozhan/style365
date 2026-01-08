using Style365.Domain.Entities;

namespace Style365.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repository properties
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    IShoppingCartRepository ShoppingCarts { get; }
    IWishlistRepository Wishlists { get; }
    IPaymentRepository Payments { get; }
    IProductReviewRepository ProductReviews { get; }
    IRepository<ProductTag> ProductTags { get; }

    // Unit of Work operations
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}