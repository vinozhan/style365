using MediatR;
using Style365.Application.Common.Exceptions;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdWithProductsAsync(request.Id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException(nameof(Category), request.Id);
        }

        // Check if category has products
        if (category.Products.Any() && !request.Force)
        {
            return Result.Failure($"Category has {category.Products.Count} products. Use force=true to delete anyway.");
        }

        // Check if category has subcategories
        var subCategories = await _unitOfWork.Categories.GetSubCategoriesAsync(category.Id, cancellationToken);
        if (subCategories.Any())
        {
            return Result.Failure($"Category has {subCategories.Count()} subcategories. Delete subcategories first.");
        }

        // Soft delete the category
        category.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}