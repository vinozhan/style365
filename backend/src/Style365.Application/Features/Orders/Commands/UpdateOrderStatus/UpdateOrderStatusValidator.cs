using FluentValidation;
using Style365.Domain.Enums;

namespace Style365.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Valid status is required");

        RuleFor(x => x.TrackingNumber)
            .MaximumLength(100)
            .WithMessage("Tracking number cannot exceed 100 characters");

        RuleFor(x => x.ShippingCarrier)
            .MaximumLength(100)
            .WithMessage("Shipping carrier cannot exceed 100 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes cannot exceed 1000 characters");
    }
}