using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Application.Features.Users;

namespace TerryCorner.Api.Controllers.Admin;

public record UpdateUserRequest(string FullName, string Email, string? PhoneNumber);
public record CreateManagerAccountRequest(string FullName, string Email, string Password);
public record ChangeUserRoleRequest(string NewRole);

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class UsersAdminController(ISender mediator, IFileStorageService fileStorage) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllUsersQuery(), ct);
        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<UserStatsDto>> GetStats(CancellationToken ct)
    {
        var result = await mediator.Send(new GetUserStatsQuery(), ct);
        return Ok(result);
    }

    [HttpPost("managers")]
    public async Task<ActionResult<AdminUserDto>> CreateManager(CreateManagerAccountRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateManagerAccountCommand(request.FullName, request.Email, request.Password), ct);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateUserRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdateUserCommand(id, request.FullName, request.Email, request.PhoneNumber), ct);
        return NoContent();
    }

    [HttpPatch("{id}/role")]
    public async Task<IActionResult> ChangeRole(string id, ChangeUserRoleRequest request, CancellationToken ct)
    {
        await mediator.Send(new ChangeUserRoleCommand(id, request.NewRole), ct);
        return NoContent();
    }

    [HttpPost("{id}/restrict")]
    public async Task<IActionResult> Restrict(string id, CancellationToken ct)
    {
        await mediator.Send(new RestrictUserCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id}/unrestrict")]
    public async Task<IActionResult> Unrestrict(string id, CancellationToken ct)
    {
        await mediator.Send(new UnrestrictUserCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id}/profile-picture")]
    public async Task<IActionResult> GetProfilePicture(string id, CancellationToken ct)
    {
        var fileName = await mediator.Send(new GetProfilePictureFileNameQuery(id), ct);
        if (string.IsNullOrEmpty(fileName))
        {
            return NotFound();
        }

        var (content, contentType) = await fileStorage.OpenProfilePictureAsync(fileName, ct);
        return File(content, contentType);
    }
}
