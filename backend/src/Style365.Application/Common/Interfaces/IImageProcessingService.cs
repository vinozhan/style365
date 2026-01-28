namespace Style365.Application.Common.Interfaces;

public interface IImageProcessingService
{
    Task<ImageValidationResult> ValidateImageAsync(
        Stream imageStream,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<ProcessedImageSet> ProcessImageAsync(
        Stream imageStream,
        string originalFileName,
        CancellationToken cancellationToken = default);
}

public class ImageValidationResult
{
    public bool IsValid { get; set; }
    public string? Error { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? DetectedFormat { get; set; }

    public static ImageValidationResult Valid(int width, int height, string format) => new()
    {
        IsValid = true,
        Width = width,
        Height = height,
        DetectedFormat = format
    };

    public static ImageValidationResult Invalid(string error) => new()
    {
        IsValid = false,
        Error = error
    };
}

public class ProcessedImageSet
{
    public ProcessedImage Original { get; set; } = null!;
    public ProcessedImage ThumbnailSmall { get; set; } = null!;
    public ProcessedImage ThumbnailMedium { get; set; } = null!;
    public ProcessedImage ThumbnailLarge { get; set; } = null!;
    public ProcessedImage? WebP { get; set; }
}

public class ProcessedImage
{
    public byte[] Data { get; set; } = [];
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSize => Data.Length;
}
