using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
using Style365.Domain.Enums;

namespace Style365.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, Result<CreateReviewResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateReviewHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateReviewResponse>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        // Check if product exists
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return Result.Failure<CreateReviewResponse>("Product not found");
        }

        // Check if user already reviewed this product
        var existingReview = await _unitOfWork.ProductReviews.GetByUserAndProductAsync(request.UserId, request.ProductId, cancellationToken);
        if (existingReview != null)
        {
            return Result.Failure<CreateReviewResponse>("You have already reviewed this product");
        }

        // Check if this is user's first review (affects moderation)
        var userReviewCount = await _unitOfWork.ProductReviews.CountByUserAsync(request.UserId, cancellationToken);
        var isNewUser = userReviewCount == 0;

        // Validate order item if provided
        if (request.OrderItemId.HasValue)
        {
            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(request.OrderItemId.Value, cancellationToken);
            if (orderItem == null || orderItem.ProductId != request.ProductId)
            {
                return Result.Failure<CreateReviewResponse>("Invalid order item for this product");
            }
        }

        // Create review with smart moderation
        var review = new ProductReview(
            request.ProductId,
            request.UserId,
            request.Rating,
            request.Title,
            request.Comment,
            request.OrderItemId,
            isNewUser);

        await _unitOfWork.ProductReviews.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateReviewResponse
        {
            ReviewId = review.Id,
            Status = review.Status.ToString(),
            IsInstantlyVisible = review.Status == ReviewStatus.Published,
            Message = review.Status == ReviewStatus.Published 
                ? "Your review has been published and is now visible to other customers."
                : "Thank you for your review! It will be visible after moderation."
        };

        return Result.Success(response);
    }
}