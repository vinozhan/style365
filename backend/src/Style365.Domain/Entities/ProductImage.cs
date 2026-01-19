using Style365.Domain.Common;

namespace Style365.Domain.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string ImageUrl { get; private set; } = null!;
    public string? AltText { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsMain { get; private set; }

    public Product Product { get; private set; } = null!;

    private ProductImage() { }

    public ProductImage(string imageUrl, string? altText = null, int sortOrder = 0, bool isMain = false)
    {
        ImageUrl = ValidateImageUrl(imageUrl);
        AltText = altText?.Trim();
        IsMain = isMain;
        SortOrder = sortOrder;
    }

    public bool IsPrimary => IsMain;
    public int DisplayOrder => SortOrder;

    public void UpdateImage(string imageUrl, string? altText)
    {
        ImageUrl = ValidateImageUrl(imageUrl);
        AltText = altText?.Trim();
        UpdateTimestamp();
    }

    public void SetAsMain(bool isMain)
    {
        IsMain = isMain;
        UpdateTimestamp();
    }

    public void SetPrimary() => SetAsMain(true);
    public void UnsetPrimary() => SetAsMain(false);

    public void UpdateSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
            throw new ArgumentException("Sort order cannot be negative", nameof(sortOrder));
            
        SortOrder = sortOrder;
        UpdateTimestamp();
    }

    private static string ValidateImageUrl(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be empty", nameof(imageUrl));
            
        imageUrl = imageUrl.Trim();
        
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid image URL format", nameof(imageUrl));
            
        return imageUrl;
    }
}