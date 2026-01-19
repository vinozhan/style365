using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Reviews.Queries.GetReviewsByUser;

public class GetReviewsByUserQuery : IRequest<Result<GetReviewsByUserResponse>>
{
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}