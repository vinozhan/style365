namespace Style365.Application.Features.Products.Commands.BulkImportProducts;

public class ProductCsvRecord
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? ComparePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public double Weight { get; set; } = 0;
    public string WeightUnit { get; set; } = "kg";
    public string? Brand { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Tags { get; set; } // Pipe-separated: "Tag1|Tag2|Tag3"
    public string? Size { get; set; } // Pipe-separated for variants: "S|M|L|XL"
    public string? Color { get; set; } // Pipe-separated for variants: "Red|Blue|Green"
    public string? ImageUrls { get; set; } // Pipe-separated URLs
}
