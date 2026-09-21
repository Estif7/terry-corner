using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Features.Orders;
using TerryCorner.Application.Features.PaymentReceipts;

namespace TerryCorner.Api.Controllers;

public record CreateOrderApiRequest(
    string ContactFullName,
    string ContactPhoneNumber,
    string OrderType,
    string? DeliveryAddress,
    string? OrderNotes,
    IReadOnlyList<CreateOrderItemRequest> Items);

public record UploadReceiptApiRequest(string? TransactionReferenceNumber, string? PaymentNote);

[ApiController]
[Route("api/orders")]
[AllowAnonymous] // guest checkout is supported; ICurrentUserService resolves the user when present
public class OrdersController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderApiRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateOrderCommand(new CreateOrderRequest(
                request.ContactFullName,
                request.ContactPhoneNumber,
                request.OrderType,
                request.DeliveryAddress,
                request.OrderNotes,
                request.Items)),
            ct);

        return Ok(result);
    }

    [HttpGet("{orderNumber}")]
    public async Task<ActionResult<OrderTrackingDto>> Track(string orderNumber, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderByNumberQuery(orderNumber), ct);
        return Ok(result);
    }

    [HttpPost("{orderNumber}/receipts")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<PaymentReceiptDto>> UploadReceipt(
        string orderNumber,
        [FromForm] UploadReceiptApiRequest request,
        IFormFile file,
        CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();

        var result = await mediator.Send(
            new UploadPaymentReceiptCommand(new UploadPaymentReceiptRequest(
                orderNumber,
                stream,
                file.FileName,
                file.ContentType,
                request.TransactionReferenceNumber,
                request.PaymentNote)),
            ct);

        return Ok(result);
    }
}
