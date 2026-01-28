using Microsoft.Extensions.Logging;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Products.Commands.SetPrimaryProductImage;

public class SetPrimaryProductImageHandler : ICommandHandler<SetPrimaryProductImageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetPrimaryProductImageHandler> _logger;

    public SetPrimaryProductImageHandler(
        IUnitOfWork unitOfWork,
        ILogger<SetPrimaryProductImageHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(SetPrimaryProductImageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdWithFullDetailsAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                return Result.Failure("Product not found");
            }

            var targetImage = product.Images.FirstOrDefault(i => i.Id == request.ImageId);
            if (targetImage == null)
            {
                return Result.Failure("Image not found");
            }

            // Unset all other images as primary
            foreach (var image in product.Images)
            {
                if (image.Id != request.ImageId && image.IsPrimary)
                {
                    image.UnsetPrimary();
                }
            }

            // Set the target image as primary
            targetImage.SetPrimary();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Set image {ImageId} as primary for product {ProductId}", request.ImageId, request.ProductId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary image {ImageId} for product {ProductId}", request.ImageId, request.ProductId);
            return Result.Failure($"Failed to set primary image: {ex.Message}");
        }
    }
}
