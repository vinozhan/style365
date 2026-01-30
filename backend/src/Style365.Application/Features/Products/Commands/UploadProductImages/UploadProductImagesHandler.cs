using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Products.Commands.UploadProductImages;

public class UploadProductImagesHandler : ICommandHandler<UploadProductImagesCommand, Result<UploadProductImagesResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly S3Settings _s3Settings;
    private readonly ILogger<UploadProductImagesHandler> _logger;

    public UploadProductImagesHandler(
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        IImageProcessingService imageProcessingService,
        IOptions<S3Settings> s3Settings,
        ILogger<UploadProductImagesHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _imageProcessingService = imageProcessingService;
        _s3Settings = s3Settings.Value;
        _logger = logger;
    }

    public async Task<Result<UploadProductImagesResult>> Handle(UploadProductImagesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdWithFullDetailsAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result.Failure<UploadProductImagesResult>("Product not found");
            }

            var result = new UploadProductImagesResult
            {
                ProductId = request.ProductId
            };

            var existingImageCount = product.Images.Count;
            var sortOrder = existingImageCount;
            var isFirstImage = existingImageCount == 0;

            foreach (var file in request.Files)
            {
                try
                {
                    var uploadResult = await ProcessAndUploadImage(
                        file,
                        product,
                        request.AltText,
                        sortOrder,
                        isFirstImage && sortOrder == existingImageCount,
                        cancellationToken);

                    if (uploadResult.IsSuccess)
                    {
                        result.UploadedImages.Add(uploadResult.Data!);
                        sortOrder++;
                    }
                    else
                    {
                        result.FailedUploads.Add(new UploadFailureResult
                        {
                            FileName = file.FileName,
                            Error = string.Join(", ", uploadResult.Errors)
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing file {FileName}", file.FileName);
                    result.FailedUploads.Add(new UploadFailureResult
                    {
                        FileName = file.FileName,
                        Error = ex.Message
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading images for product {ProductId}", request.ProductId);
            return Result.Failure<UploadProductImagesResult>($"Failed to upload images: {ex.Message}");
        }
    }

    private async Task<Result<UploadedImageResult>> ProcessAndUploadImage(
        FileUploadDto file,
        Product product,
        string? altText,
        int sortOrder,
        bool isPrimary,
        CancellationToken cancellationToken)
    {
        // Validate file size
        if (file.Length > _s3Settings.MaxFileSizeBytes)
        {
            return Result.Failure<UploadedImageResult>($"File size exceeds maximum allowed ({_s3Settings.MaxFileSizeBytes / 1024 / 1024}MB)");
        }

        // Validate content type
        if (!_s3Settings.AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure<UploadedImageResult>($"Content type '{file.ContentType}' is not allowed");
        }

        // Validate image
        var validationResult = await _imageProcessingService.ValidateImageAsync(file.Stream, file.ContentType, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<UploadedImageResult>(validationResult.Error!);
        }

        // Process image to generate thumbnails
        file.Stream.Position = 0;
        var processedImages = await _imageProcessingService.ProcessImageAsync(file.Stream, file.FileName, cancellationToken);

        var folder = $"products/{product.Id}";

        // Upload all variants
        var originalUpload = await _storageService.UploadFileAsync(
            processedImages.Original.Data,
            processedImages.Original.FileName,
            processedImages.Original.ContentType,
            folder,
            cancellationToken);

        if (!originalUpload.IsSuccess)
        {
            return Result.Failure<UploadedImageResult>($"Failed to upload original image: {originalUpload.Error}");
        }

        var smallUpload = await _storageService.UploadFileAsync(
            processedImages.ThumbnailSmall.Data,
            processedImages.ThumbnailSmall.FileName,
            processedImages.ThumbnailSmall.ContentType,
            folder,
            cancellationToken);

        var mediumUpload = await _storageService.UploadFileAsync(
            processedImages.ThumbnailMedium.Data,
            processedImages.ThumbnailMedium.FileName,
            processedImages.ThumbnailMedium.ContentType,
            folder,
            cancellationToken);

        var largeUpload = await _storageService.UploadFileAsync(
            processedImages.ThumbnailLarge.Data,
            processedImages.ThumbnailLarge.FileName,
            processedImages.ThumbnailLarge.ContentType,
            folder,
            cancellationToken);

        StorageUploadResult? webPUpload = null;
        if (processedImages.WebP != null)
        {
            webPUpload = await _storageService.UploadFileAsync(
                processedImages.WebP.Data,
                processedImages.WebP.FileName,
                processedImages.WebP.ContentType,
                folder,
                cancellationToken);
        }

        // Create ProductImage entity with explicit ProductId
        var productImage = ProductImage.CreateFromS3Upload(
            productId: product.Id,
            imageUrl: originalUpload.PublicUrl!,
            s3Key: originalUpload.FileKey!,
            thumbnailSmallUrl: smallUpload.IsSuccess ? smallUpload.PublicUrl : null,
            thumbnailSmallS3Key: smallUpload.IsSuccess ? smallUpload.FileKey : null,
            thumbnailMediumUrl: mediumUpload.IsSuccess ? mediumUpload.PublicUrl : null,
            thumbnailMediumS3Key: mediumUpload.IsSuccess ? mediumUpload.FileKey : null,
            thumbnailLargeUrl: largeUpload.IsSuccess ? largeUpload.PublicUrl : null,
            thumbnailLargeS3Key: largeUpload.IsSuccess ? largeUpload.FileKey : null,
            webPUrl: webPUpload?.IsSuccess == true ? webPUpload.PublicUrl : null,
            webPS3Key: webPUpload?.IsSuccess == true ? webPUpload.FileKey : null,
            fileSize: originalUpload.FileSize,
            width: processedImages.Original.Width,
            height: processedImages.Original.Height,
            originalFileName: file.FileName,
            altText: altText,
            sortOrder: sortOrder,
            isMain: isPrimary);

        // If this is the primary image, unset primary on existing images
        if (isPrimary)
        {
            foreach (var img in product.Images.Where(i => i.IsPrimary))
            {
                img.UnsetPrimary();
            }
        }

        // Add to repository explicitly to ensure proper EF Core tracking as new entity (INSERT)
        await _unitOfWork.ProductImages.AddAsync(productImage, cancellationToken);

        return Result.Success(new UploadedImageResult
        {
            ImageId = productImage.Id,
            Url = originalUpload.PublicUrl!,
            ThumbnailSmallUrl = smallUpload.IsSuccess ? smallUpload.PublicUrl : null,
            ThumbnailMediumUrl = mediumUpload.IsSuccess ? mediumUpload.PublicUrl : null,
            ThumbnailLargeUrl = largeUpload.IsSuccess ? largeUpload.PublicUrl : null,
            WebPUrl = webPUpload?.IsSuccess == true ? webPUpload.PublicUrl : null,
            Width = processedImages.Original.Width,
            Height = processedImages.Original.Height,
            FileSize = originalUpload.FileSize,
            OriginalFileName = file.FileName,
            IsPrimary = isPrimary,
            SortOrder = sortOrder
        });
    }
}
