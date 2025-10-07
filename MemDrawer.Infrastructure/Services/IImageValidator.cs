using MemDrawer.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace MemDrawer.Infrastructure.Services;

/// <summary>
/// Validates images for format, size, and dimensions.
/// </summary>
public interface IImageValidator
{
    /// <summary>
    /// Validates the provided image stream.
    /// Checks for allowed formats (JPEG, PNG), maximum file size, and dimensions.
    /// If the image is valid but not in JPEG format, it converts it to JPEG and returns the converted stream.
    /// </summary>
    /// <param name="imageStream">The image stream to validate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see cref="ImageValidationResult"/> indicating whether the image is valid and any relevant messages or converted stream.</returns>
    Task<ImageValidationResult> ValidateImageAsync(Stream imageStream, CancellationToken cancellationToken = default);
}

public class ImageValidator(ILogger<ImageValidator> logger, IConfiguration configuration) : IImageValidator
{
    // Default limits
    private const long DefaultMaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private const long DefaultMaxImageWidth = 5000; // 5000 pixels
    private const long DefaultMaxImageHeight = 5000; // 5000 pixels

    // Allowed formats
    private static HashSet<string> AllowedFormats { get; } =
    [
        "JPEG",
        "PNG"
    ];

    private readonly string _allowedFormatString = string.Join(", ", AllowedFormats);

    public async Task<ImageValidationResult> ValidateImageAsync(Stream imageStream,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var maxFileSizeBytes = GetMaxFileSizeBytes();
            if (imageStream.Length > maxFileSizeBytes)
            {
                return new ImageValidationResult(false,
                    $"File size exceeds the maximum allowed size of {maxFileSizeBytes / (1024 * 1024)} MB.");
            }

            var contentType = await Image.DetectFormatAsync(imageStream, cancellationToken);

            if (!AllowedFormats.Contains(contentType.Name))
            {
                return new ImageValidationResult(false,
                    $"Unsupported image format. Only '{_allowedFormatString}' are allowed.");
            }

            using var image = await Image.LoadAsync(imageStream, cancellationToken);
            if (image.Width > DefaultMaxImageWidth || image.Height > DefaultMaxImageHeight)
            {
                return new ImageValidationResult(false,
                    $"Image dimensions exceed the maximum allowed size of {DefaultMaxImageWidth}x{DefaultMaxImageHeight} pixels.");
            }

            // Optionally, you can re-encode the image to ensure it's in a standard format
            // Here we re-encode to JPEG if it's not already JPEG
            if (contentType.Name == "PNG")
            {
                // Reset stream position to beginning for further processing
                imageStream.Seek(0, SeekOrigin.Begin);

                // Convert PNG to JPEG and return the new stream
                var tempStream = new MemoryStream();
                await image.SaveAsJpegAsync(tempStream, cancellationToken);
                tempStream.Seek(0, SeekOrigin.Begin);
                return new ImageValidationResult(true, ConvertedStream: tempStream);
            }

            return new ImageValidationResult(true);
        }
        catch (UnknownImageFormatException e)
        {
            logger.LogError(e, "Unsupported image format.");
            return new ImageValidationResult(false, "Unsupported image format.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error during image validation.");
            return new ImageValidationResult(false, "Error during image validation.");
        }
    }

    private long GetMaxFileSizeBytes()
    {
        var configMaxFileSizeMb = configuration.GetValue<int>("ImageConfiguration:MaxSizeInBytes");
        return configMaxFileSizeMb > 0 ? configMaxFileSizeMb : DefaultMaxFileSizeBytes;
    }
}