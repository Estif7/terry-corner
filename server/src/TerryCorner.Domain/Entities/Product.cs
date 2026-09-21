using TerryCorner.Domain.Common;

namespace TerryCorner.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Authoritative current price in ETB. Never trust a client-supplied price.</summary>
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsPopular { get; set; }
    public int SortOrder { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<ProductTopping> ProductToppings { get; set; } = new List<ProductTopping>();
}
