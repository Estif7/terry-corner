using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Entities;

namespace TerryCorner.Application.Features.Products;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    Guid CategoryId,
    bool IsFeatured,
    bool IsPopular,
    int SortOrder,
    IReadOnlyList<Guid> ToppingIds) : IRequest<ProductDto>;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    Guid CategoryId,
    bool IsAvailable,
    bool IsFeatured,
    bool IsPopular,
    int SortOrder,
    IReadOnlyList<Guid> ToppingIds) : IRequest;

public record DeleteProductCommand(Guid Id) : IRequest;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class CreateProductCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct)
            ?? throw new KeyNotFoundException($"Category {request.CategoryId} was not found.");

        var toppings = await db.Toppings.Where(t => request.ToppingIds.Contains(t.Id)).ToListAsync(ct);

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            CategoryId = request.CategoryId,
            IsFeatured = request.IsFeatured,
            IsPopular = request.IsPopular,
            SortOrder = request.SortOrder,
        };
        db.Products.Add(product);

        foreach (var topping in toppings)
        {
            db.ProductToppings.Add(new ProductTopping { Product = product, Topping = topping });
        }

        await db.SaveChangesAsync(ct);

        return new ProductDto(
            product.Id, product.Name, product.Description, product.Price, product.ImageUrl,
            product.IsAvailable, product.IsFeatured, product.IsPopular, product.CategoryId, category.Name,
            product.SortOrder,
            toppings.Select(t => new ToppingDto(t.Id, t.Name, t.AdditionalPrice, t.IsAvailable, t.SortOrder)).ToList());
    }
}

public class UpdateProductCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await db.Products
            .Include(p => p.ProductToppings)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Product {request.Id} was not found.");

        var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
        {
            throw new KeyNotFoundException($"Category {request.CategoryId} was not found.");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.ImageUrl = request.ImageUrl;
        product.CategoryId = request.CategoryId;
        product.IsAvailable = request.IsAvailable;
        product.IsFeatured = request.IsFeatured;
        product.IsPopular = request.IsPopular;
        product.SortOrder = request.SortOrder;
        product.UpdatedAtUtc = DateTime.UtcNow;

        // Sync the topping assignments: remove ones no longer selected, add newly selected ones.
        var currentToppingIds = product.ProductToppings.Select(pt => pt.ToppingId).ToHashSet();
        var desiredToppingIds = request.ToppingIds.ToHashSet();

        var toRemove = product.ProductToppings.Where(pt => !desiredToppingIds.Contains(pt.ToppingId)).ToList();
        foreach (var pt in toRemove)
        {
            db.ProductToppings.Remove(pt);
        }

        var toAddIds = desiredToppingIds.Where(id => !currentToppingIds.Contains(id)).ToList();
        if (toAddIds.Count > 0)
        {
            var toppingsToAdd = await db.Toppings.Where(t => toAddIds.Contains(t.Id)).ToListAsync(ct);
            foreach (var topping in toppingsToAdd)
            {
                db.ProductToppings.Add(new ProductTopping { ProductId = product.Id, ToppingId = topping.Id });
            }
        }

        await db.SaveChangesAsync(ct);
    }
}

public class ProductHasOrderHistoryException : Exception
{
    public ProductHasOrderHistoryException()
        : base("This product has been ordered before and can't be deleted. Mark it as sold out instead.") { }
}

public class DeleteProductCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Product {request.Id} was not found.");

        var hasOrderHistory = await db.OrderItems.AnyAsync(i => i.ProductId == request.Id, ct);
        if (hasOrderHistory)
        {
            throw new ProductHasOrderHistoryException();
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);
    }
}
