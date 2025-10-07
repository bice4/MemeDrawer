using MemDrawer.ApiService.Services;
using MemDrawer.Domain.Exceptions;
using MemDrawer.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemDrawer.ApiService.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController(
    ILogger<ImageController> logger,
    IResponseBuilder responseBuilder,
    IImageService imageService,
    IImageValidator imageValidator) : ControllerBase
{
    [HttpGet]
    [ResponseCache(Duration = 120)]
    public async Task<IActionResult> GetAllImages(CancellationToken cancellationToken)
    {
        try
        {
            var images = await imageService.GetAllImagesAsync(cancellationToken);
            return Ok(images);
        }
        catch (DomainException e)
        {
            return responseBuilder.HandleDomainException(e);
        }
        catch (Exception e)
        {
            return responseBuilder.HandleException(e);
        }
    }

    [HttpGet("/{imageId:int}")]
    public async Task<IActionResult> GetImage(int imageId, CancellationToken cancellationToken)
    {
        try
        {
            var image = await imageService.GetImageByIdAsync(imageId, cancellationToken);
            if (image is null)
            {
                return NotFound("Image with given id does not exist.");
            }

            return Ok(image);
        }
        catch (DomainException e)
        {
            return responseBuilder.HandleDomainException(e);
        }
        catch (Exception e)
        {
            return responseBuilder.HandleException(e);
        }
    }

    [HttpDelete("/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int imageId, CancellationToken cancellationToken)
    {
        try
        {
            await imageService.DeleteImageAsync(imageId, cancellationToken);
            return NoContent();
        }
        catch (DomainException e)
        {
            return responseBuilder.HandleDomainException(e);
        }
        catch (Exception e)
        {
            return responseBuilder.HandleException(e);
        }
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile formFile, CancellationToken cancellationToken)
    {
        try
        {
            if (formFile.Length == 0) return BadRequest("File is empty.");

            await using var stream = formFile.OpenReadStream();

            var imageValidationResult = await imageValidator.ValidateImageAsync(stream, cancellationToken);

            if (!imageValidationResult.IsValid)
            {
                logger.LogWarning("Image validation failed: {ErrorMessage}", imageValidationResult.ErrorMessage);
                return BadRequest(imageValidationResult.ErrorMessage);
            }

            if (imageValidationResult.ConvertedStream is not null)
            {
                await imageService.UploadImageAsync(imageValidationResult.ConvertedStream, cancellationToken);
                await imageValidationResult.ConvertedStream.DisposeAsync();
            }
            else
                await imageService.UploadImageAsync(stream, cancellationToken);

            return Created(string.Empty, null);
        }
        catch (DomainException e)
        {
            return responseBuilder.HandleDomainException(e);
        }
        catch (Exception e)
        {
            return responseBuilder.HandleException(e);
        }
    }

    [HttpPost("bathUpload")]
    public async Task<IActionResult> UploadImages([FromForm] IFormFileCollection formFile,
        CancellationToken cancellationToken)
    {
        try
        {
            if (formFile.Count == 0) return BadRequest("No files uploaded.");


            foreach (var formFileItem in formFile)
            {
                await using var stream = formFileItem.OpenReadStream();

                var imageValidationResult = await imageValidator.ValidateImageAsync(stream, cancellationToken);

                if (!imageValidationResult.IsValid)
                {
                    logger.LogWarning("Image validation failed: {ErrorMessage}", imageValidationResult.ErrorMessage);
                    return BadRequest(imageValidationResult.ErrorMessage);
                }

                if (imageValidationResult.ConvertedStream is not null)
                {
                    await imageService.UploadImageAsync(imageValidationResult.ConvertedStream, cancellationToken);
                    await imageValidationResult.ConvertedStream.DisposeAsync();
                }
                else
                    await imageService.UploadImageAsync(stream, cancellationToken);
            }
            
            return Created(string.Empty, null);
        }
        catch (DomainException e)
        {
            return responseBuilder.HandleDomainException(e);
        }
        catch (Exception e)
        {
            return responseBuilder.HandleException(e);
        }
    }
}