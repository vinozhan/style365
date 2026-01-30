using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Categories.Commands.UploadCategoryImage;

public record UploadCategoryImageCommand : ICommand<Result<UploadCategoryImageResult>>
{
    public Guid CategoryId { get; init; }
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
}

public class UploadCategoryImageResult
{
    public Guid CategoryId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public long FileSize { get; set; }
}
