using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Reviews.Queries.GetReviewsByProduct;

public class GetReviewsByProductHandler : IRequestHandler<GetReviewsByProductQuery, Result<GetReviewsByProductResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReviewsByProductHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GetReviewsByProductResponse>> Handle(GetReviewsByProductQuery request, CancellationToken cancellationToken)
    {
        // Verify product exists
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return Result.Failure<GetReviewsByProductResponse>("Product not found");
        }

        // Get paginated reviews
        var (reviews, totalCount) = await _unitOfWork.ProductReviews.GetPagedReviewsByProductAsync(
            request.ProductId,
            request.Page,
            request.PageSize,
            approvedOnly: true,
            request.SortBy,
            request.Ascending,
            cancellationToken);

        var reviewDtos = reviews.Select(r => new ProductReviewDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            UserId = r.UserId,
            UserName = r.User?.GetFullName() ?? "Anonymous",
            Rating = r.Rating,
            Title = r.Title ?? string.Empty,
            Comment = r.Comment ?? string.Empty,
            IsVerifiedPurchase = r.IsVerifiedPurchase,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();

        var response = new GetReviewsByProductResponse
        {
            Reviews = reviewDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return Result.Success(response);
    }
}