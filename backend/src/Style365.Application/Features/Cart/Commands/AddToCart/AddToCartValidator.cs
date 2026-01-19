using FluentValidation;

namespace Style365.Application.Features.Cart.Commands.AddToCart;

public class AddToCartValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100");

        RuleFor(x => x)
            .Must(HaveUserIdOrSessionId)
            .WithMessage("Either UserId or SessionId must be provided");
    }

    private bool HaveUserIdOrSessionId(AddToCartCommand command)
    {
        return command.UserId.HasValue || !string.IsNullOrWhiteSpace(command.SessionId);
    }
}