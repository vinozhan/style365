using Style365.Domain.Entities;

namespace Style365.Application.Common.Interfaces;

public interface IProductReviewRepository : IRepository<ProductReview>
{
    Task<IEnumerable<ProductReview>> GetReviewsByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductReview>> GetReviewsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductReview>> GetApprovedReviewsByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductReview>> GetPendingReviewsAsync(CancellationToken cancellationToken = default);
    Task<ProductReview?> GetReviewByUserAndProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task<ProductReview?> GetByUserAndProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task<bool> HasUserReviewedProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<double> GetAverageRatingForProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<int> GetReviewCountForProductAsync(Guid productId, bool approvedOnly = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductReview>> GetVerifiedPurchaseReviewsAsync(Guid productId, CancellationToken cancellationToken = default);
    
    // Pagination for reviews
    Task<(IEnumerable<ProductReview> Reviews, int TotalCount)> GetPagedReviewsByProductAsync(
        Guid productId,
        int page,
        int pageSize,
        bool approvedOnly = true,
        string? sortBy = null,
        bool ascending = false,
        CancellationToken cancellationToken = default);
}