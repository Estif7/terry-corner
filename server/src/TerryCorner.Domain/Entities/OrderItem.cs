using TerryCorner.Domain.Common;

namespace TerryCorner.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    /// <summary>Product name at time of order — preserved even if the product is later renamed/removed.</summary>
    public string ProductNameSnapshot { get; set; } = string.Empty;

    /// <summary>Unit price at time of order. Historical orders never depend on the current product price.</summary>
    public decimal UnitPriceSnapshot { get; set; }

    public int Quantity { get; set; }

    /// <summary>(UnitPriceSnapshot + sum of topping snapshots) * Quantity, computed server-side.</summary>
    public decimal LineTotal { get; set; }

    public ICollection<OrderItemTopping> Toppings { get; set; } = new List<OrderItemTopping>();
}

public class OrderItemTopping
{
    public Guid OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = null!;

    public Guid ToppingId { get; set; }
    public Topping Topping { get; set; } = null!;

    public string ToppingNameSnapshot { get; set; } = string.Empty;
    public decimal AdditionalPriceSnapshot { get; set; }
}
