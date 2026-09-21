using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Application.Features.Customers;
using TerryCorner.Application.Features.Users;

namespace TerryCorner.Api.Controllers;

public record UpdateProfileRequest(string FullName, string PhoneNumber);

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController(ISender mediator, IFileStorageService fileStorage, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<ActionResult<MyProfileDto>> GetProfile(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyProfileQuery(), ct);
        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdateMyProfileCommand(request.FullName, request.PhoneNumber), ct);
        return NoContent();
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<MyOrderListItemDto>>> GetOrders(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyOrdersQuery(), ct);
        return Ok(result);
    }

    [HttpGet("orders/{orderNumber}")]
    public async Task<ActionResult<MyOrderDetailDto>> GetOrderDetail(string orderNumber, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyOrderDetailQuery(orderNumber), ct);
        return Ok(result);
    }

    [HttpPost("profile-picture")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file, CancellationToken ct)
    {
        var userId = currentUser.UserId!;
        await using var stream = file.OpenReadStream();

        await mediator.Send(
            new UploadProfilePictureCommand(new UploadProfilePictureRequest(userId, stream, file.FileName, file.ContentType)),
            ct);

        return NoContent();
    }

    [HttpDelete("profile-picture")]
    public async Task<IActionResult> DeleteProfilePicture(CancellationToken ct)
    {
        await mediator.Send(new DeleteProfilePictureCommand(currentUser.UserId!), ct);
        return NoContent();
    }

    [HttpGet("profile-picture")]
    public async Task<IActionResult> GetProfilePicture(CancellationToken ct)
    {
        var fileName = await mediator.Send(new GetProfilePictureFileNameQuery(currentUser.UserId!), ct);
        if (string.IsNullOrEmpty(fileName))
        {
            return NotFound();
        }

        var (content, contentType) = await fileStorage.OpenProfilePictureAsync(fileName, ct);
        return File(content, contentType);
    }
}
