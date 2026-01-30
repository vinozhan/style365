using FluentValidation;

namespace Style365.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU is required")
            .MaximumLength(50)
            .WithMessage("SKU cannot exceed 50 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-letter code (e.g., USD)");

        RuleFor(x => x.CompareAtPrice)
            .GreaterThan(0)
            .When(x => x.CompareAtPrice.HasValue)
            .WithMessage("Compare price must be greater than 0");

        RuleFor(x => x.CostPrice)
            .GreaterThan(0)
            .When(x => x.CostPrice.HasValue)
            .WithMessage("Cost price must be greater than 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.LowStockThreshold)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Low stock threshold cannot be negative");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Category is required");

        RuleFor(x => x.Description)
            .MaximumLength(5000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 5000 characters");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.ShortDescription))
            .WithMessage("Short description cannot exceed 500 characters");

        RuleFor(x => x.Brand)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Brand))
            .WithMessage("Brand cannot exceed 100 characters");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.MetaTitle))
            .WithMessage("Meta title cannot exceed 100 characters");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(300)
            .When(x => !string.IsNullOrEmpty(x.MetaDescription))
            .WithMessage("Meta description cannot exceed 300 characters");
    }
}
