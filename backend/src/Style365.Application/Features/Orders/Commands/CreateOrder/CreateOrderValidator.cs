using FluentValidation;
using Style365.Domain.Enums;

namespace Style365.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Customer email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required")
            .SetValidator(new OrderShippingInfoValidator());

        RuleFor(x => x.BillingAddress)
            .SetValidator(new OrderShippingInfoValidator()!)
            .When(x => x.BillingAddress != null);

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method");

        RuleFor(x => x)
            .Must(HaveUserIdOrSessionId)
            .WithMessage("Either UserId or SessionId must be provided");

        RuleFor(x => x.CustomerPhone)
            .Matches(@"^\+?[\d\s\-\(\)]+$")
            .WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerPhone));
    }

    private bool HaveUserIdOrSessionId(CreateOrderCommand command)
    {
        return command.UserId.HasValue || !string.IsNullOrWhiteSpace(command.SessionId);
    }
}

public class OrderShippingInfoValidator : AbstractValidator<OrderShippingInfo>
{
    public OrderShippingInfoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required")
            .MaximumLength(100).WithMessage("Address line 1 must not exceed 100 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(50).WithMessage("City must not exceed 50 characters");

        RuleFor(x => x.StateProvince)
            .NotEmpty().WithMessage("State/Province is required")
            .MaximumLength(50).WithMessage("State/Province must not exceed 50 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(50).WithMessage("Country must not exceed 50 characters");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[\d\s\-\(\)]+$")
            .WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}