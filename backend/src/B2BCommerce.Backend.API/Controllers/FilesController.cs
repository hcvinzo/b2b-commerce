using B2BCommerce.Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.API.Controllers;

/// <summary>
/// Controller for file upload operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IStorageService _storageService;
    private readonly ILogger<FilesController> _logger;

    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    };

    private static readonly HashSet<string> AllowedDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/jpg",
        "image/png"
    };

    private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png"
    };

    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private const long MaxDocumentFileSize = 10 * 1024 * 1024; // 10MB

    public FilesController(
        IStorageService storageService,
        ILogger<FilesController> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// Upload an image file
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <param name="folder">Optional folder path (e.g., "products", "categories")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The public URL of the uploaded image</returns>
    [HttpPost("upload/image")]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadImage(
        IFormFile file,
        [FromQuery] string? folder = "products",
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        // Validate file size
        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { message = $"File size exceeds the maximum allowed size of {MaxFileSize / 1024 / 1024}MB" });
        }

        // Validate content type
        if (!AllowedImageTypes.Contains(file.ContentType))
        {
            return BadRequest(new { message = $"Invalid file type. Allowed types: {string.Join(", ", AllowedImageTypes)}" });
        }

        // Validate file extension
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = $"Invalid file extension. Allowed extensions: {string.Join(", ", AllowedExtensions)}" });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var url = await _storageService.UploadFileAsync(
                stream,
                file.FileName,
                file.ContentType,
                folder,
                cancellationToken);

            _logger.LogInformation("Image uploaded successfully: {FileName} -> {Url}", file.FileName, url);

            return Ok(new FileUploadResponse
            {
                Url = url,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image: {FileName}", file.FileName);
            return StatusCode(500, new { message = "Failed to upload image. Please try again." });
        }
    }

    /// <summary>
    /// Upload multiple image files
    /// </summary>
    /// <param name="files">The image files to upload</param>
    /// <param name="folder">Optional folder path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of uploaded file URLs</returns>
    [HttpPost("upload/images")]
    [ProducesResponseType(typeof(MultipleFileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(MaxFileSize * 10)] // Allow up to 10 files at 5MB each
    public async Task<IActionResult> UploadImages(
        List<IFormFile> files,
        [FromQuery] string? folder = "products",
        CancellationToken cancellationToken = default)
    {
        if (files is null || files.Count == 0)
        {
            return BadRequest(new { message = "No files provided" });
        }

        if (files.Count > 10)
        {
            return BadRequest(new { message = "Maximum 10 files can be uploaded at once" });
        }

        var results = new List<FileUploadResponse>();
        var errors = new List<FileUploadError>();

        foreach (var file in files)
        {
            // Validate each file
            if (file.Length == 0)
            {
                errors.Add(new FileUploadError { FileName = file.FileName, Error = "File is empty" });
                continue;
            }

            if (file.Length > MaxFileSize)
            {
                errors.Add(new FileUploadError { FileName = file.FileName, Error = "File size exceeds maximum allowed" });
                continue;
            }

            if (!AllowedImageTypes.Contains(file.ContentType))
            {
                errors.Add(new FileUploadError { FileName = file.FileName, Error = "Invalid file type" });
                continue;
            }

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
            {
                errors.Add(new FileUploadError { FileName = file.FileName, Error = "Invalid file extension" });
                continue;
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var url = await _storageService.UploadFileAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    folder,
                    cancellationToken);

                results.Add(new FileUploadResponse
                {
                    Url = url,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Size = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image: {FileName}", file.FileName);
                errors.Add(new FileUploadError { FileName = file.FileName, Error = "Upload failed" });
            }
        }

        return Ok(new MultipleFileUploadResponse
        {
            Uploaded = results,
            Errors = errors
        });
    }

    /// <summary>
    /// Upload a document file (PDF, JPG, PNG) - for authenticated users
    /// </summary>
    /// <param name="file">The document file to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The public URL of the uploaded document</returns>
    [HttpPost("upload/document")]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(MaxDocumentFileSize)]
    public async Task<IActionResult> UploadDocument(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        return await UploadDocumentInternal(file, cancellationToken);
    }

    /// <summary>
    /// Upload a document file during registration (anonymous access)
    /// Used for uploading customer documents before the customer is created
    /// </summary>
    /// <param name="file">The document file to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The public URL of the uploaded document</returns>
    [HttpPost("upload/registration-document")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(MaxDocumentFileSize)]
    public async Task<IActionResult> UploadRegistrationDocument(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        return await UploadDocumentInternal(file, cancellationToken);
    }

    private async Task<IActionResult> UploadDocumentInternal(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        // Validate file size
        if (file.Length > MaxDocumentFileSize)
        {
            return BadRequest(new { message = $"File size exceeds the maximum allowed size of {MaxDocumentFileSize / 1024 / 1024}MB" });
        }

        // Validate content type
        if (!AllowedDocumentTypes.Contains(file.ContentType))
        {
            return BadRequest(new { message = $"Invalid file type. Allowed types: PDF, JPEG, PNG" });
        }

        // Validate file extension
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedDocumentExtensions.Contains(extension))
        {
            return BadRequest(new { message = $"Invalid file extension. Allowed extensions: {string.Join(", ", AllowedDocumentExtensions)}" });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var url = await _storageService.UploadFileAsync(
                stream,
                file.FileName,
                file.ContentType,
                "files", // Documents are stored in the 'files' folder
                cancellationToken);

            _logger.LogInformation("Document uploaded successfully: {FileName} -> {Url}", file.FileName, url);

            return Ok(new FileUploadResponse
            {
                Url = url,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload document: {FileName}", file.FileName);
            return StatusCode(500, new { message = "Failed to upload document. Please try again." });
        }
    }

    /// <summary>
    /// Delete an uploaded file
    /// </summary>
    /// <param name="request">The file URL to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteFile(
        [FromBody] DeleteFileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.Url))
        {
            return BadRequest(new { message = "File URL is required" });
        }

        try
        {
            await _storageService.DeleteFileAsync(request.Url, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {Url}", request.Url);
            return StatusCode(500, new { message = "Failed to delete file" });
        }
    }
}

public record FileUploadResponse
{
    public required string Url { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public long Size { get; init; }
}

public record FileUploadError
{
    public required string FileName { get; init; }
    public required string Error { get; init; }
}

public record MultipleFileUploadResponse
{
    public required List<FileUploadResponse> Uploaded { get; init; }
    public required List<FileUploadError> Errors { get; init; }
}

public record DeleteFileRequest
{
    public string? Url { get; init; }
}
