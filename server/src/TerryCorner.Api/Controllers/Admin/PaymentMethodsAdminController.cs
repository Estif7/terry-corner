using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Features.PaymentMethods;

namespace TerryCorner.Api.Controllers.Admin;

public record CreatePaymentMethodRequest(
    string MethodName, string AccountName, string AccountNumberOrIdentifier,
    string? PhoneNumber, string Instructions, int SortOrder);

public record UpdatePaymentMethodRequest(
    string MethodName, string AccountName, string AccountNumberOrIdentifier,
    string? PhoneNumber, string Instructions, int SortOrder, bool IsActive);

[ApiController]
[Route("api/admin/payment-methods")]
[Authorize(Roles = "Manager,Admin")]
public class PaymentMethodsAdminController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminPaymentMethodDto>>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllPaymentMethodsQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentMethodDto>> Create(CreatePaymentMethodRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreatePaymentMethodCommand(
            request.MethodName, request.AccountName, request.AccountNumberOrIdentifier,
            request.PhoneNumber, request.Instructions, request.SortOrder), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdatePaymentMethodRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdatePaymentMethodCommand(
            id, request.MethodName, request.AccountName, request.AccountNumberOrIdentifier,
            request.PhoneNumber, request.Instructions, request.SortOrder, request.IsActive), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeletePaymentMethodCommand(id), ct);
        return NoContent();
    }
}
