using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Features.Promotions;

namespace TerryCorner.Api.Controllers.Admin;

public record CreatePromotionRequest(
    string Name, string? Description, string DiscountType, decimal DiscountValue,
    DateTime StartDateUtc, DateTime EndDateUtc, bool IsFeatured, IReadOnlyList<Guid> ProductIds);

public record UpdatePromotionRequest(
    string Name, string? Description, string DiscountType, decimal DiscountValue,
    DateTime StartDateUtc, DateTime EndDateUtc, bool IsFeatured, bool IsActive, IReadOnlyList<Guid> ProductIds);

[ApiController]
[Route("api/admin/promotions")]
[Authorize(Roles = "Manager,Admin")]
public class PromotionsAdminController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminPromotionDto>>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllPromotionsQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminPromotionDto>> Create(CreatePromotionRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreatePromotionCommand(
            request.Name, request.Description, request.DiscountType, request.DiscountValue,
            request.StartDateUtc, request.EndDateUtc, request.IsFeatured, request.ProductIds), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdatePromotionRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdatePromotionCommand(
            id, request.Name, request.Description, request.DiscountType, request.DiscountValue,
            request.StartDateUtc, request.EndDateUtc, request.IsFeatured, request.IsActive, request.ProductIds), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeletePromotionCommand(id), ct);
        return NoContent();
    }
}
