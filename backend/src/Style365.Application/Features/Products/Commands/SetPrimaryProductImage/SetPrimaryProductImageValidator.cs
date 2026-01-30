using FluentValidation;

namespace Style365.Application.Features.Products.Commands.SetPrimaryProductImage;

public class SetPrimaryProductImageValidator : AbstractValidator<SetPrimaryProductImageCommand>
{
    public SetPrimaryProductImageValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.ImageId)
            .NotEmpty().WithMessage("Image ID is required");
    }
}
