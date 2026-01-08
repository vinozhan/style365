using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommand : IRequest<Result<CreateCategoryResponse>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class CreateCategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}