using TerryCorner.Domain.Common;
using TerryCorner.Domain.Enums;

namespace TerryCorner.Domain.Entities;

public class Order : BaseEntity
{
    /// <summary>Human-friendly order number shown to customers, e.g. "TC-1042".</summary>
    public string OrderNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public OrderStatus Status { get; set; } = OrderStatus.OrderReceived;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public OrderType OrderType { get; set; }

    public string ContactFullName { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
    public string? DeliveryAddress { get; set; }
    public string? OrderNotes { get; set; }

    /// <summary>Server-calculated total in ETB at order-creation time. Never trust a client-supplied total.</summary>
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }

    public Guid? PromotionId { get; set; }
    public Promotion? Promotion { get; set; }

    public string? InternalStaffNotes { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();
}
