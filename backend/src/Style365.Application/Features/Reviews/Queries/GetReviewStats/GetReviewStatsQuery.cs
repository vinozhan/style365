using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Reviews.Queries.GetReviewStats;

public class GetReviewStatsQuery : IRequest<Result<GetReviewStatsResponse>>
{
    public Guid ProductId { get; set; }
}