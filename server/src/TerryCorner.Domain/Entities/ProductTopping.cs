namespace TerryCorner.Domain.Entities;

/// <summary>Join table: which toppings are offered for a given product.</summary>
public class ProductTopping
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid ToppingId { get; set; }
    public Topping Topping { get; set; } = null!;
}
