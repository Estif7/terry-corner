using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Application.Features.PaymentReceipts;

namespace TerryCorner.Api.Controllers.Admin;

public record RejectReceiptRequest(string Reason);

[ApiController]
[Route("api/admin/payment-receipts")]
[Authorize(Roles = "Manager,Admin")]
public class PaymentReceiptsAdminController(ISender mediator, IFileStorageService fileStorage) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<PaymentReceiptReviewDto>>> GetPending(CancellationToken ct)
    {
        var result = await mediator.Send(new GetPendingPaymentReceiptsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/file")]
    public async Task<IActionResult> GetFile(Guid id, CancellationToken ct)
    {
        var storedFileName = await mediator.Send(new GetPaymentReceiptStoredFileNameQuery(id), ct);
        var (content, contentType) = await fileStorage.OpenReceiptAsync(storedFileName, ct);
        return File(content, contentType);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ApprovePaymentReceiptCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, RejectReceiptRequest request, CancellationToken ct)
    {
        await mediator.Send(new RejectPaymentReceiptCommand(id, request.Reason), ct);
        return NoContent();
    }
}
