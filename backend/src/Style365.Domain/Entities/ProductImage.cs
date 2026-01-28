using Style365.Domain.Common;

namespace Style365.Domain.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string ImageUrl { get; private set; } = null!;
    public string? AltText { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsMain { get; private set; }

    // S3 Storage properties
    public string? S3Key { get; private set; }
    public string? ThumbnailSmallUrl { get; private set; }
    public string? ThumbnailMediumUrl { get; private set; }
    public string? ThumbnailLargeUrl { get; private set; }
    public string? WebPUrl { get; private set; }

    // S3 Keys for deletion
    public string? ThumbnailSmallS3Key { get; private set; }
    public string? ThumbnailMediumS3Key { get; private set; }
    public string? ThumbnailLargeS3Key { get; private set; }
    public string? WebPS3Key { get; private set; }

    // Image metadata
    public long FileSize { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public string? OriginalFileName { get; private set; }

    public Product Product { get; private set; } = null!;

    private ProductImage() { }

    public ProductImage(string imageUrl, string? altText = null, int sortOrder = 0, bool isMain = false)
    {
        ImageUrl = ValidateImageUrl(imageUrl);
        AltText = altText?.Trim();
        IsMain = isMain;
        SortOrder = sortOrder;
    }

    public static ProductImage CreateFromS3Upload(
        Guid productId,
        string imageUrl,
        string s3Key,
        string? thumbnailSmallUrl,
        string? thumbnailSmallS3Key,
        string? thumbnailMediumUrl,
        string? thumbnailMediumS3Key,
        string? thumbnailLargeUrl,
        string? thumbnailLargeS3Key,
        string? webPUrl,
        string? webPS3Key,
        long fileSize,
        int width,
        int height,
        string? originalFileName,
        string? altText = null,
        int sortOrder = 0,
        bool isMain = false)
    {
        var image = new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            S3Key = s3Key,
            ThumbnailSmallUrl = thumbnailSmallUrl,
            ThumbnailSmallS3Key = thumbnailSmallS3Key,
            ThumbnailMediumUrl = thumbnailMediumUrl,
            ThumbnailMediumS3Key = thumbnailMediumS3Key,
            ThumbnailLargeUrl = thumbnailLargeUrl,
            ThumbnailLargeS3Key = thumbnailLargeS3Key,
            WebPUrl = webPUrl,
            WebPS3Key = webPS3Key,
            FileSize = fileSize,
            Width = width,
            Height = height,
            OriginalFileName = originalFileName?.Trim(),
            AltText = altText?.Trim(),
            SortOrder = sortOrder,
            IsMain = isMain
        };

        return image;
    }

    public bool IsPrimary => IsMain;
    public int DisplayOrder => SortOrder;
    public bool IsStoredInS3 => !string.IsNullOrEmpty(S3Key);

    public IEnumerable<string> GetAllS3Keys()
    {
        var keys = new List<string>();

        if (!string.IsNullOrEmpty(S3Key))
            keys.Add(S3Key);
        if (!string.IsNullOrEmpty(ThumbnailSmallS3Key))
            keys.Add(ThumbnailSmallS3Key);
        if (!string.IsNullOrEmpty(ThumbnailMediumS3Key))
            keys.Add(ThumbnailMediumS3Key);
        if (!string.IsNullOrEmpty(ThumbnailLargeS3Key))
            keys.Add(ThumbnailLargeS3Key);
        if (!string.IsNullOrEmpty(WebPS3Key))
            keys.Add(WebPS3Key);

        return keys;
    }

    public void UpdateImage(string imageUrl, string? altText)
    {
        ImageUrl = ValidateImageUrl(imageUrl);
        AltText = altText?.Trim();
        UpdateTimestamp();
    }

    public void UpdateAltText(string? altText)
    {
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