using FluentValidation;

namespace Style365.Application.Features.Orders.Commands.UpdateTracking;

public class UpdateTrackingValidator : AbstractValidator<UpdateTrackingCommand>
{
    public UpdateTrackingValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required");

        RuleFor(x => x.TrackingNumber)
            .NotEmpty()
            .WithMessage("Tracking number is required")
            .MaximumLength(100)
            .WithMessage("Tracking number cannot exceed 100 characters");

        RuleFor(x => x.ShippingCarrier)
            .NotEmpty()
            .WithMessage("Shipping carrier is required")
            .MaximumLength(100)
            .WithMessage("Shipping carrier cannot exceed 100 characters");
    }
}