using Microsoft.EntityFrameworkCore.Storage;
using Style365.Infrastructure.Data;
using Style365.Application.Common.Interfaces;

namespace Style365.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly Style365DbContext _context;
    private IDbContextTransaction? _transaction;

    private IUserRepository? _users;
    private ICategoryRepository? _categories;
    private IProductRepository? _products;
    private IOrderRepository? _orders;
    private IShoppingCartRepository? _shoppingCarts;
    private IWishlistRepository? _wishlists;
    private IPaymentRepository? _payments;
    private IProductReviewRepository? _productReviews;

    public UnitOfWork(Style365DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    public IShoppingCartRepository ShoppingCarts => _shoppingCarts ??= new ShoppingCartRepository(_context);
    public IWishlistRepository Wishlists => _wishlists ??= new WishlistRepository(_context);
    public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
    public IProductReviewRepository ProductReviews => _productReviews ??= new ProductReviewRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // If we have an active transaction and something fails, we should rollback
            if (_transaction != null)
            {
                await RollbackTransactionAsync(cancellationToken);
            }
            throw;
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
            throw new InvalidOperationException("A transaction is already in progress");

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction in progress");

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction in progress");

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}