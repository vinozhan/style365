using Microsoft.EntityFrameworkCore;
using Style365.Domain.Entities;
using Style365.Infrastructure.Data;
using Style365.Infrastructure.Repositories.Interfaces;

namespace Style365.Infrastructure.Repositories;

public class ProductReviewRepository : Repository<ProductReview>, IProductReviewRepository
{
    public ProductReviewRepository(Style365DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductReview>> GetReviewsByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.ProductId == productId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductReview>> GetReviewsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.UserId == userId)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductReview>> GetApprovedReviewsByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.ProductId == productId && r.IsApproved)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductReview>> GetPendingReviewsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => !r.IsApproved)
            .Include(r => r.User)
            .Include(r => r.Product)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductReview?> GetReviewByUserAndProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId, cancellationToken);
    }

    public async Task<bool> HasUserReviewedProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(r => r.UserId == userId && r.ProductId == productId, cancellationToken);
    }

    public async Task<double> GetAverageRatingForProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var reviews = await _dbSet
            .Where(r => r.ProductId == productId && r.IsApproved)
            .ToListAsync(cancellationToken);

        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
    }

    public async Task<int> GetReviewCountForProductAsync(Guid productId, bool approvedOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(r => r.ProductId == productId);
        
        if (approvedOnly)
            query = query.Where(r => r.IsApproved);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductReview>> GetVerifiedPurchaseReviewsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.ProductId == productId && r.IsVerifiedPurchase && r.IsApproved)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<ProductReview> Reviews, int TotalCount)> GetPagedReviewsByProductAsync(
        Guid productId,
        int page,
        int pageSize,
        bool approvedOnly = true,
        string? sortBy = null,
        bool ascending = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(r => r.ProductId == productId)
            .Include(r => r.User)
            .AsQueryable();

        if (approvedOnly)
            query = query.Where(r => r.IsApproved);

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy?.ToLowerInvariant() switch
        {
            "rating" => ascending ? query.OrderBy(r => r.Rating) : query.OrderByDescending(r => r.Rating),
            "created" => ascending ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt),
            "helpful" => ascending ? query.OrderBy(r => r.Rating) : query.OrderByDescending(r => r.Rating), // Placeholder for helpfulness
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var reviews = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (reviews, totalCount);
    }
}