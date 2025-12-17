using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using B2BCommerce.Backend.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// AWS S3 implementation of IStorageService with CloudFront CDN support
/// </summary>
public class S3StorageService : IStorageService, IDisposable
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _cloudFrontDomain;
    private readonly string? _cloudFrontDistributionId;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(
        IConfiguration configuration,
        ILogger<S3StorageService> logger)
    {
        _logger = logger;

        var accessKeyId = configuration["AWS:AccessKeyId"]
            ?? throw new InvalidOperationException("AWS:AccessKeyId is not configured");
        var secretAccessKey = configuration["AWS:SecretAccessKey"]
            ?? throw new InvalidOperationException("AWS:SecretAccessKey is not configured");
        var region = configuration["AWS:Region"] ?? "eu-central-1";

        _bucketName = configuration["AWS:S3:BucketName"]
            ?? throw new InvalidOperationException("AWS:S3:BucketName is not configured");
        _cloudFrontDomain = configuration["AWS:CloudFront:Domain"]
            ?? throw new InvalidOperationException("AWS:CloudFront:Domain is not configured");
        _cloudFrontDistributionId = configuration["AWS:CloudFront:DistributionId"];

        var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyId, secretAccessKey);
        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(region)
        };

        _s3Client = new AmazonS3Client(awsCredentials, s3Config);
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken ct = default)
    {
        try
        {
            // Generate unique file name to avoid collisions
            var uniqueFileName = $"{Guid.NewGuid():N}_{SanitizeFileName(fileName)}";
            var key = string.IsNullOrEmpty(folder)
                ? uniqueFileName
                : $"{folder.TrimEnd('/')}/{uniqueFileName}";

            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                // Set cache control for better CDN performance
                Headers =
                {
                    CacheControl = "public, max-age=31536000" // 1 year cache
                }
            };

            await _s3Client.PutObjectAsync(putRequest, ct);

            // Return CloudFront URL
            var cloudFrontUrl = $"https://{_cloudFrontDomain}/{key}";

            _logger.LogInformation("File uploaded successfully to S3: {Key} -> {Url}", key, cloudFrontUrl);

            return cloudFrontUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to S3: {FileName}", fileName);
            throw;
        }
    }

    public async Task DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("Could not extract S3 key from URL: {FileUrl}", fileUrl);
                return;
            }

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest, ct);

            _logger.LogInformation("File deleted successfully from S3: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file from S3: {FileUrl}", fileUrl);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string fileUrl, CancellationToken ct = default)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.GetObjectMetadataAsync(request, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if file exists in S3: {FileUrl}", fileUrl);
            return false;
        }
    }

    private string? ExtractKeyFromUrl(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);

            // Handle CloudFront URL: https://{cloudfront-domain}/{key}
            if (uri.Host.Equals(_cloudFrontDomain, StringComparison.OrdinalIgnoreCase))
            {
                return uri.AbsolutePath.TrimStart('/');
            }

            // Handle direct S3 URL: https://{bucket}.s3.{region}.amazonaws.com/{key}
            if (uri.Host.Contains("s3") && uri.Host.Contains("amazonaws.com"))
            {
                return uri.AbsolutePath.TrimStart('/');
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove or replace invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // Replace spaces with underscores
        sanitized = sanitized.Replace(" ", "_");

        // Ensure reasonable length
        if (sanitized.Length > 100)
        {
            var extension = Path.GetExtension(sanitized);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(sanitized);
            sanitized = nameWithoutExt[..Math.Min(nameWithoutExt.Length, 90)] + extension;
        }

        return sanitized;
    }

    public void Dispose()
    {
        _s3Client?.Dispose();
    }
}
