using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, Result<GetCategoriesResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoriesHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GetCategoriesResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Category> categories;

        if (request.ParentId.HasValue)
        {
            categories = await _unitOfWork.Categories.GetSubCategoriesAsync(request.ParentId.Value, cancellationToken);
        }
        else if (request.ActiveOnly == true)
        {
            categories = await _unitOfWork.Categories.GetTopLevelCategoriesAsync(cancellationToken);
        }
        else
        {
            categories = await _unitOfWork.Categories.GetAllWithProductCountsAsync(cancellationToken);
        }

        var categoryDtos = new List<CategoryDto>();

        foreach (var category in categories)
        {
            var dto = await MapToCategoryDto(category, request.IncludeSubCategories, cancellationToken);
            categoryDtos.Add(dto);
        }

        return Result.Success(new GetCategoriesResponse { Categories = categoryDtos });
    }

    private async Task<CategoryDto> MapToCategoryDto(Category category, bool includeSubCategories, CancellationToken cancellationToken)
    {
        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive,
            SortOrder = category.SortOrder,
            ParentCategoryId = category.ParentCategoryId,
            ProductCount = category.Products.Count
        };

        if (includeSubCategories)
        {
            var subCategories = await _unitOfWork.Categories.GetSubCategoriesAsync(category.Id, cancellationToken);
            foreach (var subCategory in subCategories)
            {
                var subDto = await MapToCategoryDto(subCategory, includeSubCategories, cancellationToken);
                dto.SubCategories.Add(subDto);
            }
        }

        return dto;
    }
}