using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Features.Orders;

namespace TerryCorner.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/overview")]
[Authorize(Roles = "Manager,Admin")]
public class AdminOverviewController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AdminOverviewDto>> Get(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminOverviewQuery(), ct);
        return Ok(result);
    }
}
