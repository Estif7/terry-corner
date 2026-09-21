using TerryCorner.Domain.Common;
using TerryCorner.Domain.Enums;

namespace TerryCorner.Domain.Entities;

public class Promotion : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }

    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }

    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();

    public bool IsCurrentlyActive(DateTime nowUtc) =>
        IsActive && nowUtc >= StartDateUtc && nowUtc <= EndDateUtc;
}

/// <summary>Join table: which products a promotion applies to.</summary>
public class PromotionProduct
{
    public Guid PromotionId { get; set; }
    public Promotion Promotion { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
