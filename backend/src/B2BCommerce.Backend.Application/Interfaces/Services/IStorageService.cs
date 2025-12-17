namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service for file storage operations
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Uploads a file to storage and returns the public URL
    /// </summary>
    /// <param name="fileStream">The file stream to upload</param>
    /// <param name="fileName">The name of the file</param>
    /// <param name="contentType">The MIME type of the file</param>
    /// <param name="folder">Optional folder path within the bucket</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The public URL of the uploaded file</returns>
    Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="fileUrl">The URL of the file to delete</param>
    /// <param name="ct">Cancellation token</param>
    Task DeleteFileAsync(string fileUrl, CancellationToken ct = default);

    /// <summary>
    /// Checks if a file exists in storage
    /// </summary>
    /// <param name="fileUrl">The URL of the file to check</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if the file exists, false otherwise</returns>
    Task<bool> FileExistsAsync(string fileUrl, CancellationToken ct = default);
}
