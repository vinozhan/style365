using Style365.Domain.Common;
using Style365.Domain.ValueObjects;

namespace Style365.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Sku { get; private set; } = null!;
    public Money? Price { get; private set; }
    public Money? ComparePrice { get; private set; }
    public int StockQuantity { get; private set; }
    public bool TrackQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public string? Color { get; private set; }
    public string? Size { get; private set; }
    public string? Material { get; private set; }
    public double? Weight { get; private set; }
    public string? ImageUrl { get; private set; }

    public Product Product { get; private set; } = null!;

    private ProductVariant() { }

    public ProductVariant(Guid productId, string name, string sku, string? color = null, string? size = null)
    {
        ProductId = productId;
        Name = ValidateName(name);
        Sku = ValidateSku(sku);
        Color = color?.Trim();
        Size = size?.Trim();
        TrackQuantity = true;
        IsActive = true;
        StockQuantity = 0;
    }

    public void UpdateBasicInfo(string name, string? color, string? size, string? material)
    {
        Name = ValidateName(name);
        Color = color?.Trim();
        Size = size?.Trim();
        Material = material?.Trim();
        UpdateTimestamp();
    }

    public void UpdatePricing(Money? price, Money? comparePrice = null)
    {
        Price = price;
        ComparePrice = comparePrice;
        UpdateTimestamp();
    }

    public void UpdateInventory(int stockQuantity, bool trackQuantity = true)
    {
        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));

        StockQuantity = stockQuantity;
        TrackQuantity = trackQuantity;
        UpdateTimestamp();
    }

    public void SetImage(string? imageUrl)
    {
        if (!string.IsNullOrWhiteSpace(imageUrl) && !Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid image URL format", nameof(imageUrl));
            
        ImageUrl = imageUrl?.Trim();
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    public bool IsInStock() => !TrackQuantity || StockQuantity > 0;

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Variant name cannot be empty", nameof(name));
            
        return name.Trim();
    }

    private static string ValidateSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));
            
        sku = sku.Trim().ToUpperInvariant();
        
        if (sku.Length < 3)
            throw new ArgumentException("SKU must be at least 3 characters", nameof(sku));
            
        return sku;
    }
}