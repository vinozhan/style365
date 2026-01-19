using MediatR;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public bool Force { get; set; } = false; // Force delete even if category has products
}