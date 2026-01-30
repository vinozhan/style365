using FluentValidation;

namespace Style365.Application.Features.Products.Commands.UploadProductImages;

public class UploadProductImagesValidator : AbstractValidator<UploadProductImagesCommand>
{
    private const int MaxFilesPerUpload = 10;
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp",
        "image/gif"
    ];

    public UploadProductImagesValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Files)
            .NotNull().WithMessage("At least one file is required")
            .Must(files => files != null && files.Count > 0).WithMessage("At least one file is required")
            .Must(files => files == null || files.Count <= MaxFilesPerUpload)
                .WithMessage($"Maximum {MaxFilesPerUpload} files can be uploaded at once");

        RuleForEach(x => x.Files).ChildRules(file =>
        {
            file.RuleFor(f => f.Length)
                .LessThanOrEqualTo(MaxFileSizeBytes)
                .WithMessage($"File size must not exceed {MaxFileSizeBytes / 1024 / 1024}MB");

            file.RuleFor(f => f.ContentType)
                .Must(ct => AllowedContentTypes.Contains(ct, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Only JPEG, PNG, WebP, and GIF images are allowed");

            file.RuleFor(f => f.FileName)
                .NotEmpty().WithMessage("File name is required");
        });

        RuleFor(x => x.AltText)
            .MaximumLength(300).WithMessage("Alt text must not exceed 300 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.AltText));
    }
}
