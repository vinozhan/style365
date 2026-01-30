using FluentValidation;

namespace Style365.Application.Features.Products.Commands.BulkImportProducts;

public class BulkImportProductsValidator : AbstractValidator<BulkImportProductsCommand>
{
    private static readonly string[] AllowedExtensions = [".csv"];

    public BulkImportProductsValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("CSV file is required");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .Must(IsValidExtension).WithMessage("File must have .csv extension");
    }

    private static bool IsValidExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var extension = Path.GetExtension(fileName);
        return AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }
}
