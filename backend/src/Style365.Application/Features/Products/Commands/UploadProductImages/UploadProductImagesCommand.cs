using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Products.Commands.UploadProductImages;

public record UploadProductImagesCommand : ICommand<Result<UploadProductImagesResult>>
{
    public Guid ProductId { get; init; }
    public List<FileUploadDto> Files { get; init; } = [];
    public string? AltText { get; init; }
}

public class FileUploadDto
{
    public Stream Stream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; }
}

public class UploadProductImagesResult
{
    public Guid ProductId { get; set; }
    public List<UploadedImageResult> UploadedImages { get; set; } = [];
    public List<UploadFailureResult> FailedUploads { get; set; } = [];
    public int TotalUploaded => UploadedImages.Count;
    public int TotalFailed => FailedUploads.Count;
}

public class UploadedImageResult
{
    public Guid ImageId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailSmallUrl { get; set; }
    public string? ThumbnailMediumUrl { get; set; }
    public string? ThumbnailLargeUrl { get; set; }
    public string? WebPUrl { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSize { get; set; }
    public string? OriginalFileName { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public class UploadFailureResult
{
    public string FileName { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}
