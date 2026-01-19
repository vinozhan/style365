using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommand : IRequest<Result<CreateReviewResponse>>
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public Guid? OrderItemId { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}

public class CreateReviewResponse
{
    public Guid ReviewId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsInstantlyVisible { get; set; }
    public string Message { get; set; } = string.Empty;
}