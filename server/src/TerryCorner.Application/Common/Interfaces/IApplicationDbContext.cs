using Microsoft.EntityFrameworkCore;
using TerryCorner.Domain.Entities;

namespace TerryCorner.Application.Common.Interfaces;

/// <summary>
/// Narrow abstraction over the EF Core DbContext. Application handlers depend on this
/// interface, not on TerryCorner.Infrastructure — keeping dependencies pointing inward.
/// Implemented by TerryCorner.Infrastructure.Persistence.TerryCornerDbContext.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<Topping> Toppings { get; }
    DbSet<ProductTopping> ProductToppings { get; }
    DbSet<Customer> Customers { get; }
    DbSet<CustomerAddress> CustomerAddresses { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderItemTopping> OrderItemToppings { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    DbSet<PaymentReceipt> PaymentReceipts { get; }
    DbSet<PaymentMethod> PaymentMethods { get; }
    DbSet<Promotion> Promotions { get; }
    DbSet<PromotionProduct> PromotionProducts { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
