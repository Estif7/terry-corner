using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Features.PaymentMethods;

namespace TerryCorner.Api.Controllers;

[ApiController]
[Route("api/payment-methods")]
[AllowAnonymous]
public class PaymentMethodsController(ISender mediator) : ControllerBase
{
    [HttpGet("active")]
    public async Task<ActionResult<IReadOnlyList<PaymentMethodDto>>> GetActive(CancellationToken ct)
    {
        var result = await mediator.Send(new GetActivePaymentMethodsQuery(), ct);
        return Ok(result);
    }
}
