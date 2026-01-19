using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Users.Queries.GetCustomers;

public record GetCustomersQuery : IQuery<Result<PaginatedResult<CustomerDto>>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; } = "CreatedAt";
    public bool Ascending { get; init; } = false;
}
