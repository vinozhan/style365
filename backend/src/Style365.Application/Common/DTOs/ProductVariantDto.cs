namespace Style365.Application.Common.DTOs;

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public decimal? ComparePrice { get; set; }
    public int StockQuantity { get; set; }
    public bool TrackQuantity { get; set; }
    public bool IsActive { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public double? Weight { get; set; }
    public string? ImageUrl { get; set; }

    public bool IsInStock => !TrackQuantity || StockQuantity > 0;
}