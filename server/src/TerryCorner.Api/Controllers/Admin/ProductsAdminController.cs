using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Features.Products;

namespace TerryCorner.Api.Controllers.Admin;

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    Guid CategoryId,
    bool IsFeatured,
    bool IsPopular,
    int SortOrder,
    IReadOnlyList<Guid> ToppingIds);

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    Guid CategoryId,
    bool IsAvailable,
    bool IsFeatured,
    bool IsPopular,
    int SortOrder,
    IReadOnlyList<Guid> ToppingIds);

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Manager,Admin")]
public class ProductsAdminController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateProductCommand(
            request.Name, request.Description, request.Price, request.ImageUrl, request.CategoryId,
            request.IsFeatured, request.IsPopular, request.SortOrder, request.ToppingIds), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdateProductCommand(
            id, request.Name, request.Description, request.Price, request.ImageUrl, request.CategoryId,
            request.IsAvailable, request.IsFeatured, request.IsPopular, request.SortOrder, request.ToppingIds), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(id), ct);
        return NoContent();
    }
}
