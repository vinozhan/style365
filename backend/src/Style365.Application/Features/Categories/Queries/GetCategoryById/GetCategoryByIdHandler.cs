using MediatR;
using Style365.Application.Common.Exceptions;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Application.Features.Categories.Queries.GetCategories;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoryByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = request.IncludeProducts
            ? await _unitOfWork.Categories.GetByIdWithProductsAsync(request.Id, cancellationToken)
            : request.IncludeSubCategories
                ? await _unitOfWork.Categories.GetByIdWithSubCategoriesAsync(request.Id, cancellationToken)
                : await _unitOfWork.Categories.GetByIdAsync(request.Id);

        if (category == null)
        {
            throw new NotFoundException(nameof(Category), request.Id);
        }

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

        if (request.IncludeSubCategories)
        {
            var subCategories = await _unitOfWork.Categories.GetSubCategoriesAsync(category.Id, cancellationToken);
            foreach (var subCategory in subCategories)
            {
                dto.SubCategories.Add(new CategoryDto
                {
                    Id = subCategory.Id,
                    Name = subCategory.Name,
                    Slug = subCategory.Slug,
                    Description = subCategory.Description,
                    ImageUrl = subCategory.ImageUrl,
                    IsActive = subCategory.IsActive,
                    SortOrder = subCategory.SortOrder,
                    ParentCategoryId = subCategory.ParentCategoryId,
                    ProductCount = subCategory.Products.Count
                });
            }
        }

        return Result.Success(dto);
    }
}