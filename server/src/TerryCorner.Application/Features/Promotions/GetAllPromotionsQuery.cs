using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.Promotions;

public record AdminPromotionDto(
    Guid Id,
    string Name,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    DateTime StartDateUtc,
    DateTime EndDateUtc,
    bool IsFeatured,
    bool IsActive,
    IReadOnlyList<Guid> ProductIds);

public record GetAllPromotionsQuery : IRequest<IReadOnlyList<AdminPromotionDto>>;

public class GetAllPromotionsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetAllPromotionsQuery, IReadOnlyList<AdminPromotionDto>>
{
    public async Task<IReadOnlyList<AdminPromotionDto>> Handle(GetAllPromotionsQuery request, CancellationToken ct)
    {
        var promotions = await db.Promotions
            .Include(p => p.PromotionProducts)
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(ct);

        return promotions
            .Select(p => new AdminPromotionDto(
                p.Id, p.Name, p.Description, p.DiscountType.ToString(), p.DiscountValue,
                p.StartDateUtc, p.EndDateUtc, p.IsFeatured, p.IsActive,
                p.PromotionProducts.Select(pp => pp.ProductId).ToList()))
            .ToList();
    }
}
