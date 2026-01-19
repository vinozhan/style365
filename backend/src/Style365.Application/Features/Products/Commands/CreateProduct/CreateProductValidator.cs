using FluentValidation;

namespace Style365.Application.Features.Products.Commands.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MinimumLength(2).WithMessage("Product name must be at least 2 characters")
            .MaximumLength(300).WithMessage("Product name must not exceed 300 characters");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MinimumLength(3).WithMessage("SKU must be at least 3 characters")
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters (ISO 4217)");

        RuleFor(x => x.ComparePrice)
            .GreaterThan(0).WithMessage("Compare price must be greater than 0")
            .When(x => x.ComparePrice.HasValue);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price must be 0 or greater")
            .When(x => x.CostPrice.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be 0 or greater");

        RuleFor(x => x.LowStockThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Low stock threshold must be 0 or greater");

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("Weight must be 0 or greater");

        RuleFor(x => x.WeightUnit)
            .NotEmpty().WithMessage("Weight unit is required")
            .MaximumLength(10).WithMessage("Weight unit must not exceed 10 characters");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500).WithMessage("Short description must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.ShortDescription));

        RuleFor(x => x.Brand)
            .MaximumLength(200).WithMessage("Brand must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Brand));

        RuleFor(x => x.MetaTitle)
            .MaximumLength(300).WithMessage("Meta title must not exceed 300 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.MetaTitle));

        RuleFor(x => x.MetaDescription)
            .MaximumLength(500).WithMessage("Meta description must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.MetaDescription));

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");
    }
}