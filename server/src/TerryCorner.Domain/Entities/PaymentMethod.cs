using TerryCorner.Domain.Common;

namespace TerryCorner.Domain.Entities;

public class PaymentMethod : BaseEntity
{
    public string MethodName { get; set; } = string.Empty; // e.g. "Telebirr", "Bank Transfer - CBE"
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumberOrIdentifier { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
