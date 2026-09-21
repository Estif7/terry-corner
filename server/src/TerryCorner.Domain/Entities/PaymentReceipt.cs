using TerryCorner.Domain.Common;
using TerryCorner.Domain.Enums;

namespace TerryCorner.Domain.Entities;

public class PaymentReceipt : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    /// <summary>Server-generated safe storage filename/key. Never trust or expose the original client filename.</summary>
    public string StoredFileName { get; set; } = string.Empty;
    public string OriginalFileNameForDisplay { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    public string? TransactionReferenceNumber { get; set; }
    public string? PaymentNote { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.ReceiptSubmitted;

    public string? RejectionReason { get; set; }

    /// <summary>Staff user id who reviewed this receipt (approved or rejected).</summary>
    public string? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
}
