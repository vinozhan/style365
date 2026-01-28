namespace Style365.Application.Common.Models;

public class S3Settings
{
    public const string SectionName = "AWS:S3";

    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string EnvironmentPrefix { get; set; } = "dev";
    public bool UseCloudFront { get; set; } = false;
    public string? CloudFrontDomain { get; set; }
    public long MaxFileSizeBytes { get; set; } = 10485760; // 10MB default
    public string[] AllowedContentTypes { get; set; } = ["image/jpeg", "image/png", "image/webp", "image/gif"];
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    public string GetObjectUrl(string key)
    {
        if (UseCloudFront && !string.IsNullOrEmpty(CloudFrontDomain))
        {
            return $"https://{CloudFrontDomain}/{key}";
        }
        return $"https://{BucketName}.s3.{Region}.amazonaws.com/{key}";
    }

    public string GetFullKey(string relativePath)
    {
        return $"{EnvironmentPrefix}/{relativePath}";
    }
}
