using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQuery : IRequest<Result<GetCategoriesResponse>>
{
    public bool? ActiveOnly { get; set; } = true;
    public bool IncludeSubCategories { get; set; } = false;
    public Guid? ParentId { get; set; }
}

public class GetCategoriesResponse
{
    public List<CategoryDto> Categories { get; set; } = new();
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public List<CategoryDto> SubCategories { get; set; } = new();
    public int ProductCount { get; set; }
}