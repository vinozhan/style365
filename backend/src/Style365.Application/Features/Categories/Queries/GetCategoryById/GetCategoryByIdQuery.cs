using MediatR;
using Style365.Application.Common.Models;
using Style365.Application.Features.Categories.Queries.GetCategories;

namespace Style365.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQuery : IRequest<Result<CategoryDto>>
{
    public Guid Id { get; set; }
    public bool IncludeSubCategories { get; set; } = false;
    public bool IncludeProducts { get; set; } = false;
}