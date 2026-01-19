using MediatR;
using Style365.Application.Common.Exceptions;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
using System.Text.RegularExpressions;

namespace Style365.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id);
        if (category == null)
        {
            throw new NotFoundException(nameof(Category), request.Id);
        }

        // Check for circular reference if parent is being changed
        if (request.ParentCategoryId.HasValue && request.ParentCategoryId != category.ParentCategoryId)
        {
            if (request.ParentCategoryId == category.Id)
            {
                return Result.Failure("A category cannot be its own parent");
            }

            // Check if the new parent is a descendant of this category
            if (await IsDescendantOf(request.ParentCategoryId.Value, category.Id))
            {
                return Result.Failure("Cannot set a descendant category as parent (circular reference)");
            }

            var parentCategory = await _unitOfWork.Categories.GetByIdAsync(request.ParentCategoryId.Value);
            if (parentCategory == null)
            {
                return Result.Failure("Parent category not found");
            }
        }

        // Update slug if name changed
        if (category.Name != request.Name)
        {
            var newSlug = GenerateSlug(request.Name);
            var isUnique = await _unitOfWork.Categories.IsSlugUniqueAsync(newSlug, category.Id, cancellationToken);
            
            if (!isUnique)
            {
                newSlug = $"{newSlug}-{Guid.NewGuid().ToString()[..8]}";
            }

            category.UpdateSlug(newSlug);
        }

        category.Update(
            request.Name,
            request.Description,
            request.ImageUrl,
            request.IsActive,
            request.SortOrder,
            request.ParentCategoryId
        );

        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    private async Task<bool> IsDescendantOf(Guid categoryId, Guid potentialAncestorId)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
        
        while (category != null && category.ParentCategoryId.HasValue)
        {
            if (category.ParentCategoryId == potentialAncestorId)
            {
                return true;
            }
            
            category = await _unitOfWork.Categories.GetByIdAsync(category.ParentCategoryId.Value);
        }

        return false;
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant();
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = Regex.Replace(slug, @"-{2,}", "-");
        slug = slug.Trim('-');
        return slug;
    }
}