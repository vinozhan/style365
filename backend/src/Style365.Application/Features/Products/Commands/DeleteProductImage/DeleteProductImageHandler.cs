using Microsoft.Extensions.Logging;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Products.Commands.DeleteProductImage;

public class DeleteProductImageHandler : ICommandHandler<DeleteProductImageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly ILogger<DeleteProductImageHandler> _logger;

    public DeleteProductImageHandler(
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        ILogger<DeleteProductImageHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdWithFullDetailsAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result.Failure("Product not found");
            }

            var image = product.Images.FirstOrDefault(i => i.Id == request.ImageId);
            if (image == null)
            {
                return Result.Failure("Image not found");
            }

            // Delete from S3 if stored there
            if (image.IsStoredInS3)
            {
                var s3Keys = image.GetAllS3Keys().ToList();
                if (s3Keys.Count > 0)
                {
                    var deleteResult = await _storageService.DeleteFilesAsync(s3Keys, cancellationToken);
                    if (!deleteResult)
                    {
                        _logger.LogWarning("Failed to delete some S3 files for image {ImageId}", request.ImageId);
                    }
                }
            }

            // Remove from product
            product.RemoveImage(image);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted image {ImageId} from product {ProductId}", request.ImageId, request.ProductId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image {ImageId} from product {ProductId}", request.ImageId, request.ProductId);
            return Result.Failure($"Failed to delete image: {ex.Message}");
        }
    }
}
