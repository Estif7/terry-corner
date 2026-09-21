using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerryCorner.Application.Features.Categories;

namespace TerryCorner.Api.Controllers.Admin;

public record CreateCategoryRequest(string Name, string? Description, string? ImageUrl, int SortOrder);
public record UpdateCategoryRequest(string Name, string? Description, string? ImageUrl, int SortOrder, bool IsActive);

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Manager,Admin")]
public class CategoriesAdminController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoriesQuery(ActiveOnly: false), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateCategoryCommand(request.Name, request.Description, request.ImageUrl, request.SortOrder), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdateCategoryCommand(
            id, request.Name, request.Description, request.ImageUrl, request.SortOrder, request.IsActive), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteCategoryCommand(id), ct);
        return NoContent();
    }
}
