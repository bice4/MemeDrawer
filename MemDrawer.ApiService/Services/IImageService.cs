using MemDrawer.Contracts.Http.Api.v1.Models;
using MemDrawer.Contracts.Http.Api.v1.Requests;
using MemDrawer.Domain;
using MemDrawer.Domain.Exceptions;
using MemDrawer.Infrastructure.Database;
using MemDrawer.Infrastructure.Helpers;
using MemDrawer.Infrastructure.Models;
using MemDrawer.Infrastructure.Services;
using MemeDrawer.AzureBlobServices;
using Microsoft.EntityFrameworkCore;

namespace MemDrawer.ApiService.Services;

/// <summary>
/// Service for managing images, including uploading, retrieving, deleting, and drawing text on images.
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Uploads an image from the provided stream to Azure Blob Storage and saves its metadata in the database.
    /// Checks for duplicate images using MD5 hash before uploading.
    /// </summary>
    /// <param name="stream">The stream containing the image data.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task UploadImageAsync(Stream stream, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an image by its ID, returning its metadata and content in Base64 format.
    /// Returns null if the image does not exist.
    /// </summary>
    /// <param name="imageId">The ID of the image to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see cref="ImageDto"/> if found; otherwise, null.</returns>
    Task<ImageDto?> GetImageByIdAsync(int imageId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all images, returning a list of their metadata and content in Base64 format.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of <see cref="ImageDto"/>.</returns>
    Task<List<ImageDto>> GetAllImagesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an image by its ID from both Azure Blob Storage and the database.
    /// Throws NotFoundException if the image does not exist.
    /// </summary>
    /// <param name="imageId">The ID of the image to delete.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task DeleteImageAsync(int imageId, CancellationToken cancellationToken);

    /// <summary>
    /// Draws text on an image specified by the request parameters and returns the modified image as a stream.
    /// Supports customization of text position, colors, opacity, and outline.
    /// </summary>
    /// <param name="request"><see cref="DrawOnImageRequest"/> containing drawing parameters.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Stream containing the modified image.</returns>
    Task<Stream> DrawOnImageAsync(DrawOnImageRequest request, CancellationToken cancellationToken);
}

///<inheritdoc cref="IImageService"/>
public class ImageService(ILogger<ImageService> logger, AppDbContext appDbContext, IAzureBlobService azureBlobService)
    : IImageService
{
    private const string ContentTypeJpeg = "image/jpeg";
    
    ///<inheritdoc />
    public async Task UploadImageAsync(Stream stream, CancellationToken cancellationToken)
    {
        logger.LogInformation("Upload image");

        var blobName = Guid.NewGuid().ToString("N");

        stream.Seek(0, SeekOrigin.Begin);

        // Compute MD5 hash of the image stream to check for duplicates
        var md5Hash = UnsafeMd5.Compute(stream);
        if (await ImageWithMd5HashExists(md5Hash, cancellationToken))
            throw new AlreadyExistsException("Image with given content already exists.");

        await using var transaction = await appDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            stream.Seek(0, SeekOrigin.Begin);

            // Upload the image stream to Azure Blob Storage
            // Assuming the image is in JPEG format; adjust ContentType if necessary
            await azureBlobService.UploadContentAsStreamAsync(blobName, stream, ContentTypeJpeg, cancellationToken);

            // Create a new Image record in the database
            // with the blob name and MD5 hash
            var photo = new Image(blobName, md5Hash);

            appDbContext.Images.Add(photo);
            await appDbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Photo with id: {Id} and blob with name: {BlobFileName} has been uploaded.",
                photo.Id, photo.BlobFileName);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    ///<inheritdoc />
    public async Task<ImageDto?> GetImageByIdAsync(int imageId, CancellationToken cancellationToken)
    {
        // Retrieve the image metadata from the database
        var image = await appDbContext.Images.SingleOrDefaultAsync(x => x.Id == imageId, cancellationToken);
        if (image == null) return null;

        // Retrieve the image content from Azure Blob Storage as Base64
        var contentBase64 = await azureBlobService.GetContentInBase64Async(image.BlobFileName, cancellationToken);
        if (contentBase64 == null) return null;

        return new ImageDto(image.Id, contentBase64);
    }

    ///<inheritdoc />
    public async Task<List<ImageDto>> GetAllImagesAsync(CancellationToken cancellationToken)
    {
        // Retrieve all image metadata from the database
        var images = await appDbContext.Images
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var imageDtos = new List<ImageDto>(images.Count);

        // Retrieve each image content from Azure Blob Storage as Base64
        foreach (var image in images)
        {
            var contentBase64 = await azureBlobService.GetContentInBase64Async(image.BlobFileName, cancellationToken);
            if (contentBase64 != null)
                imageDtos.Add(new ImageDto(image.Id, contentBase64));
        }

        return imageDtos;
    }

    ///<inheritdoc />
    public async Task DeleteImageAsync(int imageId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Delete image with id: {ImageId}", imageId);

        // Retrieve the image metadata from the database
        var image = await appDbContext.Images.SingleOrDefaultAsync(x => x.Id == imageId, cancellationToken);
        if (image == null) throw new NotFoundException("Image", imageId.ToString());

        // Start a transaction to ensure both blob and database record are deleted together
        await using var transaction = await appDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(image.BlobFileName))
                await azureBlobService.DeleteIfExistsAsync(image.BlobFileName, cancellationToken);

            appDbContext.Images.Remove(image);
            await appDbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Image with id: {ImageId} has been deleted", imageId);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }

    ///<inheritdoc />
    public async Task<Stream> DrawOnImageAsync(DrawOnImageRequest request, CancellationToken cancellationToken)
    {
        // Retrieve the image metadata from the database
        var image = await appDbContext.Images.SingleOrDefaultAsync(x => x.Id == request.ImageId,
            cancellationToken: cancellationToken);
        if (image == null) throw new NotFoundException("Image", request.ImageId.ToString());

        // Retrieve the image content from Azure Blob Storage as a stream
        var contentStream = await azureBlobService.GetContentAsStreamAsync(image.BlobFileName, cancellationToken);
        if (contentStream == null) throw new NotFoundException("Image blob", image.BlobFileName);

        // Prepare a memory stream to hold the result
        var resultStream = new MemoryStream();

        // Determine drawing options based on request parameters
        ImageDrawerOptions drawOptions;

        // If colors are not specified, use defaults
        if (string.IsNullOrWhiteSpace(request.BackgroundColorHex) || string.IsNullOrWhiteSpace(request.TextColorHex))
        {
            drawOptions = new ImageDrawerOptions();
        }
        else
        {
            // If background opacity is specified, include it in the options
            if (request.BackgroundOpacity.HasValue)
            {
                drawOptions = new ImageDrawerOptions(request.TextColorHex, request.BackgroundColorHex,
                    request.WithOutline,
                    request.BackgroundOpacity.Value);
            }
            else
            {
                drawOptions = new ImageDrawerOptions(request.TextColorHex, request.BackgroundColorHex, 
                    request.WithOutline);
            }
        }

        // Draw text on the image using SkiaSharp implementation
        await SkiaTextDrawer.DrawTextOnImageAsync(
            contentStream, 
            resultStream,
            request.TopText.AsMemory(),
            request.BottomText.AsMemory(), 
            drawOptions, 
            cancellationToken);
        
        // Alternatively, you can use ImageSharpTextDrawer by uncommenting the relevant lines
        // await ImageSharpTextDrawer.DrawTextOnImageAsync(request.TopText.AsMemory(),
        //     request.BottomText.AsMemory(),
        //     contentStream,
        //     resultStream,
        //     drawOptions,
        //     cancellationToken);
        
        resultStream.Seek(0, SeekOrigin.Begin);
        return resultStream;
    }

    /// Checks if an image with the given MD5 hash already exists in the database.
    private async Task<bool> ImageWithMd5HashExists(string md5Hash, CancellationToken cancellationToken) =>
        await appDbContext.Images.AnyAsync(e => e.Md5Hash == md5Hash, cancellationToken);
}