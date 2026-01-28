using FluentValidation;

namespace Style365.Application.Features.Products.Commands.DeleteProductImage;

public class DeleteProductImageValidator : AbstractValidator<DeleteProductImageCommand>
{
    public DeleteProductImageValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.ImageId)
            .NotEmpty().WithMessage("Image ID is required");
    }
}
