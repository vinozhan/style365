namespace Style365.Application.Common.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? ComparePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool TrackQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public double Weight { get; set; }
    public string? WeightUnit { get; set; }
    public string? Brand { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public string CategoryName { get; set; } = string.Empty;
    public List<ProductImageDto> Images { get; set; } = [];
    public List<ProductVariantDto> Variants { get; set; } = [];
    public List<string> Tags { get; set; } = [];

    // Computed properties
    public bool IsInStock => !TrackQuantity || StockQuantity > 0;
    public bool IsLowStock => TrackQuantity && StockQuantity <= LowStockThreshold;
    public decimal? DiscountPercentage => ComparePrice > 0 ? ((ComparePrice - Price) / ComparePrice) * 100 : null;
    public string MainImageUrl => Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? Images.FirstOrDefault()?.ImageUrl ?? "";
}