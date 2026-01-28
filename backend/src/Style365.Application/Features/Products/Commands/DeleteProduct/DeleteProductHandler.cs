using Microsoft.Extensions.Logging;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductHandler : ICommandHandler<DeleteProductCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly ILogger<DeleteProductHandler> _logger;

    public DeleteProductHandler(
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        ILogger<DeleteProductHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdWithFullDetailsAsync(request.Id, cancellationToken);

        if (product == null)
        {
            return Result.Failure<bool>($"Product with ID {request.Id} not found");
        }

        try
        {
            // Delete all product images from S3
            foreach (var image in product.Images)
            {
                if (!string.IsNullOrEmpty(image.S3Key))
                {
                    try
                    {
                        await _storageService.DeleteFileAsync(image.S3Key);

                        // Delete thumbnail variants
                        var basePath = image.S3Key.Replace(Path.GetFileName(image.S3Key), "");
                        var fileName = Path.GetFileNameWithoutExtension(image.S3Key);
                        var extension = Path.GetExtension(image.S3Key);

                        // Try to delete all variants (they may not exist)
                        await TryDeleteFileAsync($"{basePath}{fileName}_thumb_small{extension}");
                        await TryDeleteFileAsync($"{basePath}{fileName}_thumb_medium{extension}");
                        await TryDeleteFileAsync($"{basePath}{fileName}_thumb_large{extension}");
                        await TryDeleteFileAsync($"{basePath}{fileName}.webp");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete image {S3Key} from S3", image.S3Key);
                        // Continue with deletion even if S3 cleanup fails
                    }
                }
            }

            // Soft delete the product (sets IsDeleted = true)
            product.MarkAsDeleted();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product {ProductId} deleted successfully", request.Id);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", request.Id);
            return Result.Failure<bool>($"Error deleting product: {ex.Message}");
        }
    }

    private async Task TryDeleteFileAsync(string key)
    {
        try
        {
            await _storageService.DeleteFileAsync(key);
        }
        catch
        {
            // Ignore errors for variant deletion
        }
    }
}
