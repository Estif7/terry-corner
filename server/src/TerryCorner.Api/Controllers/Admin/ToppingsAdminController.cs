using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Features.Toppings;

namespace TerryCorner.Api.Controllers.Admin;

public record CreateToppingRequest(string Name, decimal AdditionalPrice, int SortOrder);
public record UpdateToppingRequest(string Name, decimal AdditionalPrice, int SortOrder, bool IsAvailable);

[ApiController]
[Route("api/admin/toppings")]
[Authorize(Roles = "Manager,Admin")]
public class ToppingsAdminController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllToppingsQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateToppingRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateToppingCommand(request.Name, request.AdditionalPrice, request.SortOrder), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateToppingRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdateToppingCommand(
            id, request.Name, request.AdditionalPrice, request.SortOrder, request.IsAvailable), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteToppingCommand(id), ct);
        return NoContent();
    }
}
