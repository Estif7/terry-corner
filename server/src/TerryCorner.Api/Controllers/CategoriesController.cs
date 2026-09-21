using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Features.Categories;

namespace TerryCorner.Api.Controllers;

[ApiController]
[Route("api/categories")]
[AllowAnonymous]
public class CategoriesController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoriesQuery(), ct);
        return Ok(result);
    }
}
