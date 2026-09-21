using Microsoft.AspNetCore.Identity;

namespace TerryCorner.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    /// <summary>Server-generated safe filename, same pattern as PaymentReceipt.StoredFileName. Never the original upload name.</summary>
    public string? ProfilePictureFileName { get; set; }

    public DateTime? CreatedAtUtc { get; set; }
}

public static class Roles
{
    public const string Customer = "Customer";
    public const string Staff = "Staff";
    public const string Manager = "Manager";
    public const string Admin = "Admin";

    public static readonly string[] All = { Customer, Staff, Manager, Admin };
    public static readonly string[] StaffLevelAndAbove = { Staff, Manager, Admin };
}
