using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Infrastructure.ImageProcessing;

public class ImageSharpProcessingService : IImageProcessingService
{
    private readonly ImageProcessingSettings _settings;
    private readonly ILogger<ImageSharpProcessingService> _logger;

    private static readonly HashSet<string> SupportedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    public ImageSharpProcessingService(
        IOptions<ImageProcessingSettings> settings,
        ILogger<ImageSharpProcessingService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ImageValidationResult> ValidateImageAsync(
        Stream imageStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!SupportedContentTypes.Contains(contentType))
            {
                return ImageValidationResult.Invalid($"Unsupported content type: {contentType}");
            }

            imageStream.Position = 0;

            var imageInfo = await Image.IdentifyAsync(imageStream, cancellationToken);
            if (imageInfo == null)
            {
                return ImageValidationResult.Invalid("Unable to identify image format");
            }

            if (imageInfo.Width > _settings.MaxImageWidth || imageInfo.Height > _settings.MaxImageHeight)
            {
                return ImageValidationResult.Invalid(
                    $"Image dimensions ({imageInfo.Width}x{imageInfo.Height}) exceed maximum allowed ({_settings.MaxImageWidth}x{_settings.MaxImageHeight})");
            }

            var format = imageInfo.Metadata.DecodedImageFormat?.Name ?? "Unknown";

            imageStream.Position = 0;

            return ImageValidationResult.Valid(imageInfo.Width, imageInfo.Height, format);
        }
        catch (UnknownImageFormatException)
        {
            return ImageValidationResult.Invalid("Unknown or unsupported image format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating image: {Message}", ex.Message);
            return ImageValidationResult.Invalid($"Image validation failed: {ex.Message}");
        }
    }

    public async Task<ProcessedImageSet> ProcessImageAsync(
        Stream imageStream,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        imageStream.Position = 0;

        using var image = await Image.LoadAsync(imageStream, cancellationToken);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

        var result = new ProcessedImageSet
        {
            Original = await CreateProcessedImage(image.Clone(x => { }), fileNameWithoutExtension, "original", null, cancellationToken),
            ThumbnailSmall = await CreateThumbnail(image, fileNameWithoutExtension, _settings.ThumbnailSmallWidth, "small", cancellationToken),
            ThumbnailMedium = await CreateThumbnail(image, fileNameWithoutExtension, _settings.ThumbnailMediumWidth, "medium", cancellationToken),
            ThumbnailLarge = await CreateThumbnail(image, fileNameWithoutExtension, _settings.ThumbnailLargeWidth, "large", cancellationToken)
        };

        if (_settings.GenerateWebP)
        {
            result.WebP = await CreateWebPVersion(image, fileNameWithoutExtension, _settings.ThumbnailLargeWidth, cancellationToken);
        }

        _logger.LogInformation(
            "Processed image {FileName}: Original ({OW}x{OH}), Small ({SW}x{SH}), Medium ({MW}x{MH}), Large ({LW}x{LH}){WebP}",
            originalFileName,
            result.Original.Width, result.Original.Height,
            result.ThumbnailSmall.Width, result.ThumbnailSmall.Height,
            result.ThumbnailMedium.Width, result.ThumbnailMedium.Height,
            result.ThumbnailLarge.Width, result.ThumbnailLarge.Height,
            result.WebP != null ? $", WebP ({result.WebP.Width}x{result.WebP.Height})" : "");

        return result;
    }

    private async Task<ProcessedImage> CreateThumbnail(
        Image image,
        string baseFileName,
        int targetWidth,
        string suffix,
        CancellationToken cancellationToken)
    {
        var clone = image.Clone(x =>
        {
            var ratio = (double)targetWidth / image.Width;
            var targetHeight = (int)(image.Height * ratio);

            x.Resize(new ResizeOptions
            {
                Size = new Size(targetWidth, targetHeight),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3
            });
        });

        return await CreateProcessedImage(clone, baseFileName, suffix, targetWidth, cancellationToken);
    }

    private async Task<ProcessedImage> CreateProcessedImage(
        Image image,
        string baseFileName,
        string suffix,
        int? targetWidth,
        CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();

        var encoder = new JpegEncoder { Quality = _settings.JpegQuality };
        await image.SaveAsJpegAsync(memoryStream, encoder, cancellationToken);

        var fileName = $"{baseFileName}_{suffix}.jpg";

        return new ProcessedImage
        {
            Data = memoryStream.ToArray(),
            FileName = fileName,
            ContentType = "image/jpeg",
            Width = image.Width,
            Height = image.Height
        };
    }

    private async Task<ProcessedImage> CreateWebPVersion(
        Image image,
        string baseFileName,
        int targetWidth,
        CancellationToken cancellationToken)
    {
        var clone = image.Clone(x =>
        {
            if (image.Width > targetWidth)
            {
                var ratio = (double)targetWidth / image.Width;
                var targetHeight = (int)(image.Height * ratio);

                x.Resize(new ResizeOptions
                {
                    Size = new Size(targetWidth, targetHeight),
                    Mode = ResizeMode.Max,
                    Sampler = KnownResamplers.Lanczos3
                });
            }
        });

        using var memoryStream = new MemoryStream();

        var encoder = new WebpEncoder { Quality = _settings.WebPQuality };
        await image.SaveAsWebpAsync(memoryStream, encoder, cancellationToken);

        var fileName = $"{baseFileName}_webp.webp";

        return new ProcessedImage
        {
            Data = memoryStream.ToArray(),
            FileName = fileName,
            ContentType = "image/webp",
            Width = clone.Width,
            Height = clone.Height
        };
    }
}
