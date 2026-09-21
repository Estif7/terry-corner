using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.Categories;

public record CategoryDto(Guid Id, string Name, string? Description, string? ImageUrl, int SortOrder, bool IsActive);

public record GetCategoriesQuery(bool ActiveOnly = true) : IRequest<IReadOnlyList<CategoryDto>>;

public class GetCategoriesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var query = db.Categories.AsNoTracking().AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(c => c.IsActive);
        }

        var categories = await query.OrderBy(c => c.SortOrder).ToListAsync(ct);

        return categories
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.ImageUrl, c.SortOrder, c.IsActive))
            .ToList();
    }
}
