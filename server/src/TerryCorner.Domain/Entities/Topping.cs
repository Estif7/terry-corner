using TerryCorner.Domain.Common;

namespace TerryCorner.Domain.Entities;

public class Topping : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Authoritative current additional price in ETB.</summary>
    public decimal AdditionalPrice { get; set; }

    public bool IsAvailable { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<ProductTopping> ProductToppings { get; set; } = new List<ProductTopping>();
}
