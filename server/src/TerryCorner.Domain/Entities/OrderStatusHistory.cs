using TerryCorner.Domain.Common;
using TerryCorner.Domain.Enums;

namespace TerryCorner.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public OrderStatus Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }

    /// <summary>Who triggered the transition — a staff user id, or "system" for automatic transitions.</summary>
    public string ChangedBy { get; set; } = string.Empty;

    public string? Reason { get; set; }
}
