using AutoMapper;
using Style365.Application.Common.DTOs;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Products.Queries.GetProducts;

public class GetProductsHandler : IQueryHandler<GetProductsQuery, Result<PaginatedResult<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (products, totalCount) = await _unitOfWork.Products.GetProductsWithFiltersAsync(
                request.Page,
                request.PageSize,
                request.CategoryId,
                request.Brand,
                request.MinPrice,
                request.MaxPrice,
                request.InStock,
                request.FeaturedOnly,
                request.SearchTerm,
                request.SortBy,
                request.Ascending,
                cancellationToken);

            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            var result = PaginatedResult<ProductDto>.Create(
                productDtos, 
                request.Page, 
                request.PageSize, 
                totalCount);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginatedResult<ProductDto>>($"Failed to retrieve products: {ex.Message}");
        }
    }
}