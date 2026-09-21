using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Entities;

namespace TerryCorner.Application.Features.Products;

public record GetProductsQuery(Guid? CategoryId, string? Search, bool FeaturedOnly)
    : IRequest<IReadOnlyList<ProductDto>>;

public class GetProductsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var query = db.Products
            .Include(p => p.Category)
            .Include(p => p.ProductToppings).ThenInclude(pt => pt.Topping)
            .AsNoTracking()
            .AsQueryable();

        if (request.CategoryId is { } categoryId)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{term}%")
                || EF.Functions.Like(p.Description, $"%{term}%"));
        }

        if (request.FeaturedOnly)
        {
            query = query.Where(p => p.IsFeatured);
        }

        var products = await query
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

        return products.Select(ToDto).ToList();
    }

    private static ProductDto ToDto(Product p) => new(
        p.Id,
        p.Name,
        p.Description,
        p.Price,
        p.ImageUrl,
        p.IsAvailable,
        p.IsFeatured,
        p.IsPopular,
        p.CategoryId,
        p.Category.Name,
        p.SortOrder,
        p.ProductToppings
            .Select(pt => new ToppingDto(pt.Topping.Id, pt.Topping.Name, pt.Topping.AdditionalPrice, pt.Topping.IsAvailable, pt.Topping.SortOrder))
            .ToList());
}
