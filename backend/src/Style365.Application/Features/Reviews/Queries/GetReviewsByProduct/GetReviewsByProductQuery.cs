using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Reviews.Queries.GetReviewsByProduct;

public class GetReviewsByProductQuery : IRequest<Result<GetReviewsByProductResponse>>
{
    public Guid ProductId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool Ascending { get; set; } = false;
}