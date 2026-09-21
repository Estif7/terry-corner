using TerryCorner.Domain.Common;

namespace TerryCorner.Domain.Entities;

public class Customer : BaseEntity
{
    /// <summary>Null for guest checkout — customer accounts are optional per spec.</summary>
    public string? ApplicationUserId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<CustomerAddress> SavedAddresses { get; set; } = new List<CustomerAddress>();
}

public class CustomerAddress : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public string Label { get; set; } = string.Empty; // e.g. "Home", "Work"
    public string AddressLine { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
