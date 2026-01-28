namespace Style365.Application.Common.DTOs;

public class ProductImageDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }

    // S3 Thumbnail URLs
    public string? ThumbnailSmallUrl { get; set; }
    public string? ThumbnailMediumUrl { get; set; }
    public string? ThumbnailLargeUrl { get; set; }
    public string? WebPUrl { get; set; }

    // Image metadata
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? OriginalFileName { get; set; }
}