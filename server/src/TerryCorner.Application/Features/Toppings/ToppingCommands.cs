using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Application.Features.Products;
using TerryCorner.Domain.Entities;

namespace TerryCorner.Application.Features.Toppings;

public record CreateToppingCommand(string Name, decimal AdditionalPrice, int SortOrder) : IRequest<ToppingDto>;

public record UpdateToppingCommand(
    Guid Id, string Name, decimal AdditionalPrice, int SortOrder, bool IsAvailable) : IRequest;

public record DeleteToppingCommand(Guid Id) : IRequest;

public class CreateToppingCommandValidator : AbstractValidator<CreateToppingCommand>
{
    public CreateToppingCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AdditionalPrice).GreaterThanOrEqualTo(0);
    }
}

public class UpdateToppingCommandValidator : AbstractValidator<UpdateToppingCommand>
{
    public UpdateToppingCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AdditionalPrice).GreaterThanOrEqualTo(0);
    }
}

public class CreateToppingCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateToppingCommand, ToppingDto>
{
    public async Task<ToppingDto> Handle(CreateToppingCommand request, CancellationToken ct)
    {
        var topping = new Topping
        {
            Name = request.Name,
            AdditionalPrice = request.AdditionalPrice,
            SortOrder = request.SortOrder,
        };
        db.Toppings.Add(topping);
        await db.SaveChangesAsync(ct);

        return new ToppingDto(topping.Id, topping.Name, topping.AdditionalPrice, topping.IsAvailable, topping.SortOrder);
    }
}

public class UpdateToppingCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateToppingCommand>
{
    public async Task Handle(UpdateToppingCommand request, CancellationToken ct)
    {
        var topping = await db.Toppings.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Topping {request.Id} was not found.");

        topping.Name = request.Name;
        topping.AdditionalPrice = request.AdditionalPrice;
        topping.SortOrder = request.SortOrder;
        topping.IsAvailable = request.IsAvailable;
        topping.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}

public class DeleteToppingCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteToppingCommand>
{
    public async Task Handle(DeleteToppingCommand request, CancellationToken ct)
    {
        var topping = await db.Toppings.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Topping {request.Id} was not found.");

        db.Toppings.Remove(topping);
        await db.SaveChangesAsync(ct);
    }
}
