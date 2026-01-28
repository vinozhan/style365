namespace Style365.Application.Common.Interfaces;

public interface IStorageService
{
    Task<StorageUploadResult> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default);

    Task<StorageUploadResult> UploadFileAsync(
        byte[] fileData,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default);

    Task<bool> DeleteFilesAsync(IEnumerable<string> fileKeys, CancellationToken cancellationToken = default);

    string GetPublicUrl(string fileKey);

    Task<bool> FileExistsAsync(string fileKey, CancellationToken cancellationToken = default);
}

public class StorageUploadResult
{
    public bool IsSuccess { get; set; }
    public string? FileKey { get; set; }
    public string? PublicUrl { get; set; }
    public long FileSize { get; set; }
    public string? Error { get; set; }

    public static StorageUploadResult Success(string fileKey, string publicUrl, long fileSize) => new()
    {
        IsSuccess = true,
        FileKey = fileKey,
        PublicUrl = publicUrl,
        FileSize = fileSize
    };

    public static StorageUploadResult Failure(string error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}
