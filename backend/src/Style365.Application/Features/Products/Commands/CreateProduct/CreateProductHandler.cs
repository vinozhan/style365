using AutoMapper;
using Style365.Application.Common.DTOs;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
using Style365.Domain.ValueObjects;

namespace Style365.Application.Features.Products.Commands.CreateProduct;

public class CreateProductHandler : ICommandHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProductHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if SKU is unique
            var skuExists = !await _unitOfWork.Products.IsSkuUniqueAsync(request.Sku, cancellationToken: cancellationToken);
            if (skuExists)
            {
                return Result.Failure<ProductDto>("SKU is already in use");
            }

            // Check if category exists
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result.Failure<ProductDto>("Category not found");
            }

            // Create price value object
            var price = Money.Create(request.Price, request.Currency);
            
            // Create compare price if provided
            Money? comparePrice = null;
            if (request.ComparePrice.HasValue)
            {
                comparePrice = Money.Create(request.ComparePrice.Value, request.Currency);
            }

            // Create cost price if provided
            Money? costPrice = null;
            if (request.CostPrice.HasValue)
            {
                costPrice = Money.Create(request.CostPrice.Value, request.Currency);
            }

            // Create product
            var product = new Product(request.Name, request.Sku, price, request.CategoryId, request.Description);

            // Update additional properties
            product.UpdatePricing(price, comparePrice, costPrice);
            product.UpdateInventory(request.StockQuantity, request.LowStockThreshold, request.TrackQuantity);
            product.UpdatePhysicalProperties(request.Weight, request.WeightUnit);
            product.UpdateBasicInfo(request.Name, request.Description, request.ShortDescription, request.Brand);
            product.UpdateSeo(request.MetaTitle, request.MetaDescription);
            product.SetFeatured(request.IsFeatured);

            if (!request.IsActive)
            {
                product.Deactivate();
            }

            // Add tags
            foreach (var tagId in request.TagIds)
            {
                var tag = await _unitOfWork.ProductTags.GetByIdAsync(tagId, cancellationToken);
                if (tag != null)
                {
                    product.AddTag(tag);
                }
            }

            // Add variants
            foreach (var variantDto in request.Variants)
            {
                var variantPrice = Money.Create(variantDto.Price, request.Currency);
                var variant = new ProductVariant(
                    variantDto.Name,
                    variantDto.Sku,
                    variantPrice,
                    variantDto.StockQuantity
                );
                variant.UpdateAttributes(variantDto.Size, variantDto.Color, variantDto.Material, variantDto.Style);
                if (!variantDto.IsActive)
                {
                    variant.Deactivate();
                }
                product.AddVariant(variant);
            }

            // Add images
            foreach (var imageDto in request.Images)
            {
                var image = new ProductImage(
                    imageDto.ImageUrl,
                    imageDto.AltText,
                    imageDto.DisplayOrder,
                    imageDto.IsPrimary
                );
                product.AddImage(image);
            }

            // Add to repository
            await _unitOfWork.Products.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Get product with category for DTO mapping
            var createdProduct = await _unitOfWork.Products.GetByIdWithFullDetailsAsync(product.Id, cancellationToken);
            var productDto = _mapper.Map<ProductDto>(createdProduct);

            return Result.Success(productDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<ProductDto>($"Failed to create product: {ex.Message}");
        }
    }
}