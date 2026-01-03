using Style365.Domain.Common;
using Style365.Domain.ValueObjects;

namespace Style365.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ShortDescription { get; private set; }
    public string Sku { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public Money? ComparePrice { get; private set; }
    public Money? CostPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public int LowStockThreshold { get; private set; }
    public bool TrackQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsFeatured { get; private set; }
    public double Weight { get; private set; }
    public string? WeightUnit { get; private set; }
    public string? Brand { get; private set; }
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public Guid CategoryId { get; private set; }

    public Category Category { get; private set; } = null!;
    private readonly List<ProductImage> _images = [];
    private readonly List<ProductVariant> _variants = [];
    private readonly List<ProductTag> _tags = [];

    public IReadOnlyList<ProductImage> Images => _images.AsReadOnly();
    public IReadOnlyList<ProductVariant> Variants => _variants.AsReadOnly();
    public IReadOnlyList<ProductTag> Tags => _tags.AsReadOnly();

    private Product() { }

    public Product(string name, string sku, Money price, Guid categoryId, string? description = null)
    {
        Name = ValidateName(name);
        Slug = GenerateSlug(name);
        Sku = ValidateSku(sku);
        Price = price;
        CategoryId = categoryId;
        Description = description?.Trim();
        TrackQuantity = true;
        IsActive = true;
        LowStockThreshold = 5;
        Weight = 0;
        WeightUnit = "kg";
    }

    public void UpdateBasicInfo(string name, string? description, string? shortDescription, string? brand)
    {
        Name = ValidateName(name);
        Slug = GenerateSlug(name);
        Description = description?.Trim();
        ShortDescription = shortDescription?.Trim();
        Brand = brand?.Trim();
        UpdateTimestamp();
    }

    public void UpdatePricing(Money price, Money? comparePrice = null, Money? costPrice = null)
    {
        Price = price;
        ComparePrice = comparePrice;
        CostPrice = costPrice;
        UpdateTimestamp();
    }

    public void UpdateInventory(int stockQuantity, int lowStockThreshold, bool trackQuantity = true)
    {
        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));
        if (lowStockThreshold < 0)
            throw new ArgumentException("Low stock threshold cannot be negative", nameof(lowStockThreshold));

        StockQuantity = stockQuantity;
        LowStockThreshold = lowStockThreshold;
        TrackQuantity = trackQuantity;
        UpdateTimestamp();
    }

    public void UpdatePhysicalProperties(double weight, string weightUnit)
    {
        if (weight < 0)
            throw new ArgumentException("Weight cannot be negative", nameof(weight));
        if (string.IsNullOrWhiteSpace(weightUnit))
            throw new ArgumentException("Weight unit cannot be empty", nameof(weightUnit));

        Weight = weight;
        WeightUnit = weightUnit.Trim().ToLowerInvariant();
        UpdateTimestamp();
    }

    public void UpdateSeo(string? metaTitle, string? metaDescription)
    {
        MetaTitle = metaTitle?.Trim();
        MetaDescription = metaDescription?.Trim();
        UpdateTimestamp();
    }

    public void SetCategory(Guid categoryId)
    {
        CategoryId = categoryId;
        UpdateTimestamp();
    }

    public void SetFeatured(bool isFeatured)
    {
        IsFeatured = isFeatured;
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

    public void ReduceStock(int quantity)
    {
        if (!TrackQuantity) return;
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
            
        if (StockQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock");

        StockQuantity -= quantity;
        UpdateTimestamp();
    }

    public void IncreaseStock(int quantity)
    {
        if (!TrackQuantity) return;
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        StockQuantity += quantity;
        UpdateTimestamp();
    }

    public bool IsInStock() => !TrackQuantity || StockQuantity > 0;
    public bool IsLowStock() => TrackQuantity && StockQuantity <= LowStockThreshold;
    public decimal? GetDiscountPercentage() => ComparePrice?.Amount > 0 ? ((ComparePrice.Amount - Price.Amount) / ComparePrice.Amount) * 100 : null;

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));
            
        name = name.Trim();
        
        if (name.Length < 2)
            throw new ArgumentException("Product name must be at least 2 characters", nameof(name));
            
        return name;
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

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
                   .Replace(" ", "-")
                   .Replace("&", "and")
                   .ToCharArray()
                   .Where(c => char.IsLetterOrDigit(c) || c == '-')
                   .Aggregate("", (current, c) => current + c);
    }
}