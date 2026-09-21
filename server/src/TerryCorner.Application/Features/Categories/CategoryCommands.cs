using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Entities;

namespace TerryCorner.Application.Features.Categories;

public record CreateCategoryCommand(
    string Name, string? Description, string? ImageUrl, int SortOrder) : IRequest<CategoryDto>;

public record UpdateCategoryCommand(
    Guid Id, string Name, string? Description, string? ImageUrl, int SortOrder, bool IsActive) : IRequest;

public record DeleteCategoryCommand(Guid Id) : IRequest;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CategoryHasProductsException : Exception
{
    public CategoryHasProductsException()
        : base("This category still has products assigned to it. Move or delete them first.") { }
}

public class CreateCategoryCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            SortOrder = request.SortOrder,
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        return new CategoryDto(category.Id, category.Name, category.Description, category.ImageUrl, category.SortOrder, category.IsActive);
    }
}

public class UpdateCategoryCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateCategoryCommand>
{
    public async Task Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Category {request.Id} was not found.");

        category.Name = request.Name;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}

public class DeleteCategoryCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Category {request.Id} was not found.");

        var hasProducts = await db.Products.AnyAsync(p => p.CategoryId == request.Id, ct);
        if (hasProducts)
        {
            throw new CategoryHasProductsException();
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
    }
}
