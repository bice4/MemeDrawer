using MemDrawer.ApiService.Services;
using MemDrawer.Contracts.Http.Api.v1.Requests;
using MemDrawer.Domain.Exceptions;
using MemDrawer.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemDrawer.ApiService.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageDrawerController(IImageService imageService, IResponseBuilder responseBuilder) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> DrawImage([FromBody] DrawOnImageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var image = await imageService.DrawOnImageAsync(request, cancellationToken);
            return File(image, "image/jpeg");
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