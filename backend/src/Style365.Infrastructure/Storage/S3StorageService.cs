using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Infrastructure.Storage;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Settings _settings;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(
        IAmazonS3 s3Client,
        IOptions<S3Settings> settings,
        ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<StorageUploadResult> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Capture length before upload as stream may be consumed
            var fileLength = fileStream.Length;

            var fileKey = GenerateFileKey(folder, fileName);
            var fullKey = _settings.GetFullKey(fileKey);

            var request = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = fullKey,
                InputStream = fileStream,
                ContentType = contentType
                // Note: ACLs are disabled on bucket. Public access is controlled via bucket policy.
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);

            var publicUrl = _settings.GetObjectUrl(fullKey);

            _logger.LogInformation("Successfully uploaded file {FileName} to S3 at {Key}", fileName, fullKey);

            return StorageUploadResult.Success(fullKey, publicUrl, fileLength);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 error uploading file {FileName}: {Message}", fileName, ex.Message);
            return StorageUploadResult.Failure($"S3 error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}: {Message}", fileName, ex.Message);
            return StorageUploadResult.Failure($"Upload error: {ex.Message}");
        }
    }

    public async Task<StorageUploadResult> UploadFileAsync(
        byte[] fileData,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(fileData);
        return await UploadFileAsync(stream, fileName, contentType, folder, cancellationToken);
    }

    public async Task<bool> DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = fileKey
            };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);

            _logger.LogInformation("Successfully deleted file from S3: {Key}", fileKey);
            return true;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 error deleting file {Key}: {Message}", fileKey, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {Key}: {Message}", fileKey, ex.Message);
            return false;
        }
    }

    public async Task<bool> DeleteFilesAsync(IEnumerable<string> fileKeys, CancellationToken cancellationToken = default)
    {
        try
        {
            var keysList = fileKeys.ToList();
            if (keysList.Count == 0)
                return true;

            var request = new DeleteObjectsRequest
            {
                BucketName = _settings.BucketName,
                Objects = keysList.Select(k => new KeyVersion { Key = k }).ToList()
            };

            var response = await _s3Client.DeleteObjectsAsync(request, cancellationToken);

            if (response.DeleteErrors.Count > 0)
            {
                foreach (var error in response.DeleteErrors)
                {
                    _logger.LogWarning("Failed to delete S3 object {Key}: {Message}", error.Key, error.Message);
                }
                return false;
            }

            _logger.LogInformation("Successfully deleted {Count} files from S3", keysList.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting multiple files from S3: {Message}", ex.Message);
            return false;
        }
    }

    public string GetPublicUrl(string fileKey)
    {
        return _settings.GetObjectUrl(fileKey);
    }

    public async Task<bool> FileExistsAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _settings.BucketName,
                Key = fileKey
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if file exists {Key}: {Message}", fileKey, ex.Message);
            return false;
        }
    }

    private static string GenerateFileKey(string folder, string fileName)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var sanitizedFileName = SanitizeFileName(fileName);
        return $"{folder}/{timestamp}_{uniqueId}_{sanitizedFileName}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Concat(fileName.Where(c => !invalidChars.Contains(c)));
        return sanitized.Replace(" ", "_").ToLowerInvariant();
    }
}
