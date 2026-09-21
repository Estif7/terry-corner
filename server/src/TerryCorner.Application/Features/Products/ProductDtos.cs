namespace TerryCorner.Application.Features.Products;

public record ToppingDto(Guid Id, string Name, decimal AdditionalPrice, bool IsAvailable, int SortOrder);

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    bool IsFeatured,
    bool IsPopular,
    Guid CategoryId,
    string CategoryName,
    int SortOrder,
    IReadOnlyList<ToppingDto> Toppings);
