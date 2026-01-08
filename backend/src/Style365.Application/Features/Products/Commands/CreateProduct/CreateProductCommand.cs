using Style365.Application.Common.DTOs;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand : ICommand<Result<ProductDto>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ShortDescription { get; init; }
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public decimal? ComparePrice { get; init; }
    public decimal? CostPrice { get; init; }
    public int StockQuantity { get; init; }
    public int LowStockThreshold { get; init; } = 5;
    public bool TrackQuantity { get; init; } = true;
    public bool IsActive { get; init; } = true;
    public bool IsFeatured { get; init; } = false;
    public double Weight { get; init; } = 0;
    public string WeightUnit { get; init; } = "kg";
    public string? Brand { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public Guid CategoryId { get; init; }
    public List<Guid> TagIds { get; init; } = new();
    public List<CreateProductVariantDto> Variants { get; init; } = new();
    public List<CreateProductImageDto> Images { get; init; } = new();
}

public class CreateProductVariantDto
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? Material { get; set; }
    public string? Style { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateProductImageDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}