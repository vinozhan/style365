using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Reviews.Queries.GetReviewsByUser;

public class GetReviewsByUserHandler : IRequestHandler<GetReviewsByUserQuery, Result<GetReviewsByUserResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReviewsByUserHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GetReviewsByUserResponse>> Handle(GetReviewsByUserQuery request, CancellationToken cancellationToken)
    {
        // Get user reviews with product information
        var allReviews = await _unitOfWork.ProductReviews.GetReviewsByUserAsync(request.UserId, cancellationToken);
        var totalCount = allReviews.Count();

        // Apply pagination
        var paginatedReviews = allReviews
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var reviewDtos = paginatedReviews.Select(r => new UserReviewDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            ProductName = r.Product?.Name ?? "Product not found",
            ProductImageUrl = r.Product?.Images?.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? string.Empty,
            Rating = r.Rating,
            Title = r.Title ?? string.Empty,
            Comment = r.Comment ?? string.Empty,
            IsVerifiedPurchase = r.IsVerifiedPurchase,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();

        var response = new GetReviewsByUserResponse
        {
            Reviews = reviewDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return Result.Success(response);
    }
}