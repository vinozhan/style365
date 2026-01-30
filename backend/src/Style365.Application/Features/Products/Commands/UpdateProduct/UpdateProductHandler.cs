using AutoMapper;
using Microsoft.Extensions.Logging;
using Style365.Application.Common.DTOs;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.ValueObjects;

namespace Style365.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductHandler : ICommandHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProductHandler> _logger;

    public UpdateProductHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateProductHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdWithFullDetailsAsync(request.Id, cancellationToken);

        if (product == null)
        {
            return Result.Failure<ProductDto>($"Product with ID {request.Id} not found");
        }

        // Check for duplicate SKU (excluding current product)
        var existingProduct = await _unitOfWork.Products.GetBySkuAsync(request.Sku, cancellationToken);
        if (existingProduct != null && existingProduct.Id != request.Id)
        {
            return Result.Failure<ProductDto>($"A product with SKU '{request.Sku}' already exists");
        }

        // Verify category exists
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result.Failure<ProductDto>($"Category with ID {request.CategoryId} not found");
        }

        try
        {
            // Update basic info
            product.UpdateBasicInfo(
                request.Name,
                request.Description,
                request.ShortDescription,
                request.Brand);

            // Update pricing
            var price = Money.Create(request.Price, request.Currency);
            var comparePrice = request.CompareAtPrice.HasValue
                ? Money.Create(request.CompareAtPrice.Value, request.Currency)
                : null;
            var costPrice = request.CostPrice.HasValue
                ? Money.Create(request.CostPrice.Value, request.Currency)
                : null;
            product.UpdatePricing(price, comparePrice, costPrice);

            // Update inventory
            product.UpdateInventory(request.StockQuantity, request.LowStockThreshold, request.TrackQuantity);

            // Update physical properties
            product.UpdatePhysicalProperties(request.Weight, request.WeightUnit);

            // Update SEO
            product.UpdateSeo(request.MetaTitle, request.MetaDescription);

            // Update category
            product.SetCategory(request.CategoryId);

            // Update featured status
            product.SetFeatured(request.IsFeatured);

            // Update active status
            if (request.IsActive)
                product.Activate();
            else
                product.Deactivate();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Re-fetch to get updated navigation properties
            product = await _unitOfWork.Products.GetByIdWithFullDetailsAsync(request.Id, cancellationToken);
            var productDto = _mapper.Map<ProductDto>(product);

            _logger.LogInformation("Product {ProductId} updated successfully", request.Id);

            return Result.Success(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", request.Id);
            return Result.Failure<ProductDto>($"Error updating product: {ex.Message}");
        }
    }
}
