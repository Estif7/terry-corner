using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.Promotions;

public record PromotionDto(
    Guid Id,
    string Name,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    bool IsFeatured,
    IReadOnlyList<Guid> ProductIds);

public record GetActivePromotionsQuery : IRequest<IReadOnlyList<PromotionDto>>;

public class GetActivePromotionsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetActivePromotionsQuery, IReadOnlyList<PromotionDto>>
{
    public async Task<IReadOnlyList<PromotionDto>> Handle(GetActivePromotionsQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var promotions = await db.Promotions
            .Include(p => p.PromotionProducts)
            .AsNoTracking()
            .Where(p => p.IsActive && p.StartDateUtc <= now && p.EndDateUtc >= now)
            .OrderByDescending(p => p.IsFeatured)
            .ToListAsync(ct);

        return promotions
            .Select(p => new PromotionDto(
                p.Id,
                p.Name,
                p.Description,
                p.DiscountType.ToString(),
                p.DiscountValue,
                p.IsFeatured,
                p.PromotionProducts.Select(pp => pp.ProductId).ToList()))
            .ToList();
    }
}
