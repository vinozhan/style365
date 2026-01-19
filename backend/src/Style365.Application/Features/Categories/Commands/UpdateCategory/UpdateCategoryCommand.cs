using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}