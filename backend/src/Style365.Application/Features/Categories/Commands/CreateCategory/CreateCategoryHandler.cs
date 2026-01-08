using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
using System.Text.RegularExpressions;

namespace Style365.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Result<CreateCategoryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateCategoryResponse>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Generate slug from name
        var slug = GenerateSlug(request.Name);

        // Check if slug already exists
        var existingCategory = await _unitOfWork.Categories.GetBySlugAsync(slug);
        if (existingCategory != null)
        {
            // Add random suffix to make it unique
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
        }

        // Validate parent category exists if provided
        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _unitOfWork.Categories.GetByIdAsync(request.ParentCategoryId.Value);
            if (parentCategory == null)
            {
                return Result.Failure<CreateCategoryResponse>("Parent category not found");
            }
        }

        var category = new Category(
            request.Name,
            slug,
            request.Description,
            request.ImageUrl,
            request.IsActive,
            request.SortOrder,
            request.ParentCategoryId
        );

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        var response = new CreateCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            ParentCategoryId = category.ParentCategoryId,
            IsActive = category.IsActive,
            SortOrder = category.SortOrder
        };

        return Result.Success(response);
    }

    private static string GenerateSlug(string name)
    {
        // Convert to lowercase
        var slug = name.ToLowerInvariant();

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove invalid characters
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Replace multiple hyphens with single hyphen
        slug = Regex.Replace(slug, @"-{2,}", "-");

        // Remove leading and trailing hyphens
        slug = slug.Trim('-');

        return slug;
    }
}