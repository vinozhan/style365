using Style365.Application.Common.DTOs;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery : IQuery<Result<PaginatedResult<ProductDto>>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public Guid? CategoryId { get; init; }
    public string? Brand { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool? InStock { get; init; }
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; } = "name";
    public bool Ascending { get; init; } = true;
    public bool ActiveOnly { get; init; } = true;
    public bool FeaturedOnly { get; init; } = false;
}