namespace Style365.Application.Common.Models;

public class ImageProcessingSettings
{
    public const string SectionName = "ImageProcessing";

    public int ThumbnailSmallWidth { get; set; } = 100;
    public int ThumbnailMediumWidth { get; set; } = 300;
    public int ThumbnailLargeWidth { get; set; } = 800;
    public int JpegQuality { get; set; } = 85;
    public bool GenerateWebP { get; set; } = true;
    public int WebPQuality { get; set; } = 80;
    public int MaxImageWidth { get; set; } = 2000;
    public int MaxImageHeight { get; set; } = 2000;
}
