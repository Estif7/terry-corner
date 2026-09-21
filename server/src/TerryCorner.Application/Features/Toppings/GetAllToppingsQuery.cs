using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Application.Features.Products;

namespace TerryCorner.Application.Features.Toppings;

public record GetAllToppingsQuery : IRequest<IReadOnlyList<ToppingDto>>;

public class GetAllToppingsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetAllToppingsQuery, IReadOnlyList<ToppingDto>>
{
    public async Task<IReadOnlyList<ToppingDto>> Handle(GetAllToppingsQuery request, CancellationToken ct)
    {
        var toppings = await db.Toppings.AsNoTracking().OrderBy(t => t.SortOrder).ToListAsync(ct);

        return toppings
            .Select(t => new ToppingDto(t.Id, t.Name, t.AdditionalPrice, t.IsAvailable, t.SortOrder))
            .ToList();
    }
}
