using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace MemeDrawer.AzureBlobServices;

public interface IAzureBlobService
{
    Task DeleteIfExistsAsync(string blobName, CancellationToken cancellationToken);
    
    Task UploadContentAsStreamAsync(string blobName, Stream contentStream, string contentType,
        CancellationToken cancellationToken);

    Task<string?> GetContentInBase64Async(string blobName, CancellationToken cancellationToken);
    
    Task<Stream?> GetContentAsStreamAsync(string blobName, CancellationToken cancellationToken);
}

public class AzureBlobService : IAzureBlobService
{
    private readonly ILogger<AzureBlobService> _logger;
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobService(ILogger<AzureBlobService> logger, BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;

        var containerClient = _blobServiceClient.GetBlobContainerClient(AzureBlobConstants.PhotoContainerName);
        containerClient.CreateIfNotExists();
    }

    public Task DeleteIfExistsAsync(string blobName, CancellationToken cancellationToken)
    {
        var blobClient = _blobServiceClient
            .GetBlobContainerClient(AzureBlobConstants.PhotoContainerName)
            .GetBlobClient(blobName);

        _logger.LogInformation("Deleting blob {BlobName}.", blobName);

        return blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public Task UploadContentAsStreamAsync(string blobName, Stream contentStream, string contentType,
        CancellationToken cancellationToken)
    {
        var blobClient = _blobServiceClient
            .GetBlobContainerClient(AzureBlobConstants.PhotoContainerName)
            .GetBlobClient(blobName);

        _logger.LogInformation("Uploading stream to blob {BlobName} with content type: {ContentType}", blobName,
            contentType);

        return blobClient.UploadAsync(contentStream, new BlobHttpHeaders()
        {
            ContentType = contentType
        }, cancellationToken: cancellationToken);
    }

    public async Task<string?> GetContentInBase64Async(string blobName, CancellationToken cancellationToken)
    {
        var blobClient = _blobServiceClient
            .GetBlobContainerClient(AzureBlobConstants.PhotoContainerName)
            .GetBlobClient(blobName);

        var response = await blobClient.DownloadContentAsync(cancellationToken);

        if (!response.HasValue) return null;

        var contentBytes = response.Value.Content;
        return Convert.ToBase64String(contentBytes);
    }
    
    public Task<Stream?> GetContentAsStreamAsync(string blobName, CancellationToken cancellationToken)
    {
        var blobClient = _blobServiceClient
            .GetBlobContainerClient(AzureBlobConstants.PhotoContainerName)
            .GetBlobClient(blobName);

        return blobClient.OpenReadAsync(cancellationToken: cancellationToken);
    }
}