using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Api.Controllers.Admin;

public record UploadImageResponse(string Url);

[ApiController]
[Route("api/admin/images")]
[Authorize(Roles = "Manager,Admin")]
public class ImagesAdminController(IFileStorageService fileStorage) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<UploadImageResponse>> Upload(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var url = await fileStorage.SavePublicImageAsync(stream, file.FileName, file.ContentType, ct);
        return Ok(new UploadImageResponse(url));
    }
}
