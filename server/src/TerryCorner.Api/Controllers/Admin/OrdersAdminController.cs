using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Features.Orders;

namespace TerryCorner.Api.Controllers.Admin;

public record UpdateOrderStatusRequest(string NewStatus);
public record AddOrderNoteRequest(string Note);

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Manager,Admin")]
public class OrdersAdminController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminOrderListItemDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] string? search, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrdersForAdminQuery(status, search), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminOrderDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderDetailForAdminQuery(id), ct);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdateOrderStatusCommand(id, request.NewStatus), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/notes")]
    public async Task<IActionResult> AddNote(Guid id, AddOrderNoteRequest request, CancellationToken ct)
    {
        await mediator.Send(new AddOrderNoteCommand(id, request.Note), ct);
        return NoContent();
    }
}
