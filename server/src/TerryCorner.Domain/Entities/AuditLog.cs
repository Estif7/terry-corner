using TerryCorner.Domain.Common;

namespace TerryCorner.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // e.g. "PaymentReceipt.Approved"
    public string EntityType { get; set; } = string.Empty; // e.g. "Order"
    public string EntityId { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>Hashed refresh tokens for JWT rotation, per user.</summary>
public class RefreshToken : BaseEntity
{
    public string ApplicationUserId { get; set; } = string.Empty;

    /// <summary>SHA-256 hash of the token — the raw token is never persisted.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
