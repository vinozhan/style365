using FluentValidation;

namespace Style365.Application.Features.Categories.Commands.UploadCategoryImage;

public class UploadCategoryImageValidator : AbstractValidator<UploadCategoryImageCommand>
{
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    ];

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public UploadCategoryImageValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Category ID is required");

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .MaximumLength(255)
            .WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required")
            .Must(BeAllowedContentType)
            .WithMessage($"Content type must be one of: {string.Join(", ", AllowedContentTypes)}");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("File cannot be empty")
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage($"File size cannot exceed {MaxFileSize / (1024 * 1024)}MB");
    }

    private static bool BeAllowedContentType(string contentType)
    {
        return AllowedContentTypes.Contains(contentType.ToLowerInvariant());
    }
}
