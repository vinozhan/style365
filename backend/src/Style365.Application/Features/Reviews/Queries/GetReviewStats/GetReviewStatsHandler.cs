using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Reviews.Queries.GetReviewStats;

public class GetReviewStatsHandler : IRequestHandler<GetReviewStatsQuery, Result<GetReviewStatsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReviewStatsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GetReviewStatsResponse>> Handle(GetReviewStatsQuery request, CancellationToken cancellationToken)
    {
        // Verify product exists
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return Result.Failure<GetReviewStatsResponse>("Product not found");
        }

        // Get all approved reviews for statistics
        var reviews = await _unitOfWork.ProductReviews.GetApprovedReviewsByProductAsync(request.ProductId, cancellationToken);
        var reviewList = reviews.ToList();

        // Calculate statistics
        var totalReviews = reviewList.Count;
        var averageRating = await _unitOfWork.ProductReviews.GetAverageRatingForProductAsync(request.ProductId, cancellationToken);
        var verifiedPurchaseReviews = reviewList.Count(r => r.IsVerifiedPurchase);

        // Calculate rating distribution
        var ratingDistribution = new RatingDistribution
        {
            FiveStar = reviewList.Count(r => r.Rating == 5),
            FourStar = reviewList.Count(r => r.Rating == 4),
            ThreeStar = reviewList.Count(r => r.Rating == 3),
            TwoStar = reviewList.Count(r => r.Rating == 2),
            OneStar = reviewList.Count(r => r.Rating == 1)
        };

        var response = new GetReviewStatsResponse
        {
            AverageRating = Math.Round(averageRating, 1),
            TotalReviews = totalReviews,
            VerifiedPurchaseReviews = verifiedPurchaseReviews,
            RatingDistribution = ratingDistribution
        };

        return Result.Success(response);
    }
}