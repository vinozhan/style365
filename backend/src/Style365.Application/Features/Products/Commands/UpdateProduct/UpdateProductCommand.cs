using Style365.Application.Common.DTOs;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand : ICommand<Result<ProductDto>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ShortDescription { get; init; }
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public decimal? CompareAtPrice { get; init; }
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
    public List<string> Tags { get; init; } = [];
}
