using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Domain.Entities;
using TerryCorner.Domain.Enums;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.Promotions;

public record CreatePromotionCommand(
    string Name,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    DateTime StartDateUtc,
    DateTime EndDateUtc,
    bool IsFeatured,
    IReadOnlyList<Guid> ProductIds) : IRequest<AdminPromotionDto>;

public record UpdatePromotionCommand(
    Guid Id,
    string Name,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    DateTime StartDateUtc,
    DateTime EndDateUtc,
    bool IsFeatured,
    bool IsActive,
    IReadOnlyList<Guid> ProductIds) : IRequest;

public record DeletePromotionCommand(Guid Id) : IRequest;

public class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DiscountType).Must(t => t is "Percentage" or "FixedAmount")
            .WithMessage("DiscountType must be 'Percentage' or 'FixedAmount'.");
        RuleFor(x => x.DiscountValue).GreaterThan(0);
        RuleFor(x => x.EndDateUtc).GreaterThan(x => x.StartDateUtc)
            .WithMessage("End date must be after the start date.");
    }
}

public class UpdatePromotionCommandValidator : AbstractValidator<UpdatePromotionCommand>
{
    public UpdatePromotionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DiscountType).Must(t => t is "Percentage" or "FixedAmount")
            .WithMessage("DiscountType must be 'Percentage' or 'FixedAmount'.");
        RuleFor(x => x.DiscountValue).GreaterThan(0);
        RuleFor(x => x.EndDateUtc).GreaterThan(x => x.StartDateUtc)
            .WithMessage("End date must be after the start date.");
    }
}

public class CreatePromotionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreatePromotionCommand, AdminPromotionDto>
{
    public async Task<AdminPromotionDto> Handle(CreatePromotionCommand request, CancellationToken ct)
    {
        var products = await db.Products.Where(p => request.ProductIds.Contains(p.Id)).ToListAsync(ct);

        var promotion = new Promotion
        {
            Name = request.Name,
            Description = request.Description,
            DiscountType = Enum.Parse<DiscountType>(request.DiscountType),
            DiscountValue = request.DiscountValue,
            StartDateUtc = request.StartDateUtc,
            EndDateUtc = request.EndDateUtc,
            IsFeatured = request.IsFeatured,
        };
        db.Promotions.Add(promotion);

        foreach (var product in products)
        {
            db.PromotionProducts.Add(new PromotionProduct { Promotion = promotion, Product = product });
        }

        await db.SaveChangesAsync(ct);

        return new AdminPromotionDto(
            promotion.Id, promotion.Name, promotion.Description, promotion.DiscountType.ToString(),
            promotion.DiscountValue, promotion.StartDateUtc, promotion.EndDateUtc, promotion.IsFeatured,
            promotion.IsActive, products.Select(p => p.Id).ToList());
    }
}

public class UpdatePromotionCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdatePromotionCommand>
{
    public async Task Handle(UpdatePromotionCommand request, CancellationToken ct)
    {
        var promotion = await db.Promotions
            .Include(p => p.PromotionProducts)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Promotion {request.Id} was not found.");

        promotion.Name = request.Name;
        promotion.Description = request.Description;
        promotion.DiscountType = Enum.Parse<DiscountType>(request.DiscountType);
        promotion.DiscountValue = request.DiscountValue;
        promotion.StartDateUtc = request.StartDateUtc;
        promotion.EndDateUtc = request.EndDateUtc;
        promotion.IsFeatured = request.IsFeatured;
        promotion.IsActive = request.IsActive;
        promotion.UpdatedAtUtc = DateTime.UtcNow;

        // Sync product assignments the same way ProductCommands syncs ProductTopping.
        var currentProductIds = promotion.PromotionProducts.Select(pp => pp.ProductId).ToHashSet();
        var desiredProductIds = request.ProductIds.ToHashSet();

        var toRemove = promotion.PromotionProducts.Where(pp => !desiredProductIds.Contains(pp.ProductId)).ToList();
        foreach (var pp in toRemove)
        {
            db.PromotionProducts.Remove(pp);
        }

        var toAddIds = desiredProductIds.Where(id => !currentProductIds.Contains(id)).ToList();
        if (toAddIds.Count > 0)
        {
            var productsToAdd = await db.Products.Where(p => toAddIds.Contains(p.Id)).ToListAsync(ct);
            foreach (var product in productsToAdd)
            {
                db.PromotionProducts.Add(new PromotionProduct { PromotionId = promotion.Id, ProductId = product.Id });
            }
        }

        await db.SaveChangesAsync(ct);
    }
}

public class DeletePromotionCommandHandler(IApplicationDbContext db) : IRequestHandler<DeletePromotionCommand>
{
    public async Task Handle(DeletePromotionCommand request, CancellationToken ct)
    {
        var promotion = await db.Promotions.FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Promotion {request.Id} was not found.");

        db.Promotions.Remove(promotion);
        await db.SaveChangesAsync(ct);
    }
}
