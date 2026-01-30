using Microsoft.Extensions.Logging;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Categories.Commands.UploadCategoryImage;

public class UploadCategoryImageHandler : ICommandHandler<UploadCategoryImageCommand, Result<UploadCategoryImageResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly ILogger<UploadCategoryImageHandler> _logger;

    public UploadCategoryImageHandler(
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        IImageProcessingService imageProcessingService,
        ILogger<UploadCategoryImageHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _imageProcessingService = imageProcessingService;
        _logger = logger;
    }

    public async Task<Result<UploadCategoryImageResult>> Handle(
        UploadCategoryImageCommand request,
        CancellationToken cancellationToken)
    {
        // Validate category exists
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result.Failure<UploadCategoryImageResult>($"Category with ID {request.CategoryId} not found");
        }

        // Validate file
        var validationResult = await _imageProcessingService.ValidateImageAsync(
            request.FileStream,
            request.ContentType,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<UploadCategoryImageResult>(validationResult.Error ?? "Invalid image file");
        }

        // Reset stream position after validation
        request.FileStream.Position = 0;

        try
        {
            // Process the image (generates thumbnails)
            var processedImages = await _imageProcessingService.ProcessImageAsync(
                request.FileStream,
                request.FileName,
                cancellationToken);

            // Use the medium thumbnail as the main category image (good size for category display)
            var mainImage = processedImages.ThumbnailMedium;

            // Upload main image to S3
            var uploadResult = await _storageService.UploadFileAsync(
                mainImage.Data,
                $"category_{request.CategoryId}_{request.FileName}",
                mainImage.ContentType,
                "categories",
                cancellationToken);

            if (!uploadResult.IsSuccess)
            {
                return Result.Failure<UploadCategoryImageResult>(uploadResult.Error ?? "Failed to upload image to storage");
            }

            // Upload thumbnail (small version)
            var thumbnailImage = processedImages.ThumbnailSmall;
            string? thumbnailUrl = null;

            var thumbnailUploadResult = await _storageService.UploadFileAsync(
                thumbnailImage.Data,
                $"category_{request.CategoryId}_thumb_{request.FileName}",
                thumbnailImage.ContentType,
                "categories",
                cancellationToken);

            if (thumbnailUploadResult.IsSuccess)
            {
                thumbnailUrl = thumbnailUploadResult.PublicUrl;
            }

            // Update category with new image URL
            category.SetImageUrl(uploadResult.PublicUrl);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully uploaded image for category {CategoryId}", request.CategoryId);

            return Result<UploadCategoryImageResult>.Success(new UploadCategoryImageResult
            {
                CategoryId = request.CategoryId,
                ImageUrl = uploadResult.PublicUrl!,
                ThumbnailUrl = thumbnailUrl,
                FileSize = uploadResult.FileSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for category {CategoryId}", request.CategoryId);
            return Result.Failure<UploadCategoryImageResult>($"Error uploading image: {ex.Message}");
        }
    }
}
