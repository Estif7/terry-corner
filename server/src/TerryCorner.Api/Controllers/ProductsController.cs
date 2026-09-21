using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Features.Products;

namespace TerryCorner.Api.Controllers;

[ApiController]
[Route("api/products")]
[AllowAnonymous]
public class ProductsController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search,
        [FromQuery] bool featuredOnly = false,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetProductsQuery(categoryId, search, featuredOnly), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), ct);
        return Ok(result);
    }
}
