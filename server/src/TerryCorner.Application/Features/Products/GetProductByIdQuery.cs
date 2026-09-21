using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.Products;

public record GetProductByIdQuery(Guid ProductId) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .Include(p => p.ProductToppings).ThenInclude(pt => pt.Topping)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new KeyNotFoundException($"Product {request.ProductId} was not found.");

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.ImageUrl,
            product.IsAvailable,
            product.IsFeatured,
            product.IsPopular,
            product.CategoryId,
            product.Category.Name,
            product.SortOrder,
            product.ProductToppings
                .Select(pt => new ToppingDto(pt.Topping.Id, pt.Topping.Name, pt.Topping.AdditionalPrice, pt.Topping.IsAvailable, pt.Topping.SortOrder))
                .ToList());
    }
}
