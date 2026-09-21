using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Features.Promotions;

namespace TerryCorner.Api.Controllers;

[ApiController]
[Route("api/promotions")]
[AllowAnonymous]
public class PromotionsController(ISender mediator) : ControllerBase
{
    [HttpGet("active")]
    public async Task<ActionResult<IReadOnlyList<PromotionDto>>> GetActive(CancellationToken ct)
    {
        var result = await mediator.Send(new GetActivePromotionsQuery(), ct);
        return Ok(result);
    }
}
