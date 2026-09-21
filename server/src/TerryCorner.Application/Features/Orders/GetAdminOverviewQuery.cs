using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Enums;

namespace TerryCorner.Application.Features.Orders;

public record PopularProductStatDto(string ProductName, int TimesOrdered);

public record AdminOverviewDto(
    int TodaysOrderCount,
    decimal TodaysRevenue,
    int PendingPaymentReceipts,
    int OrdersPreparing,
    int OrdersReady,
    int OrdersCompletedToday,
    IReadOnlyList<PopularProductStatDto> PopularProducts);

public record GetAdminOverviewQuery : IRequest<AdminOverviewDto>;

public class GetAdminOverviewQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetAdminOverviewQuery, AdminOverviewDto>
{
    public async Task<AdminOverviewDto> Handle(GetAdminOverviewQuery request, CancellationToken ct)
    {
        var todayStartUtc = DateTime.UtcNow.Date;

        var todaysOrders = await db.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAtUtc >= todayStartUtc)
            .ToListAsync(ct);

        var pendingReceipts = await db.PaymentReceipts
            .AsNoTracking()
            .CountAsync(r => r.Status == PaymentStatus.ReceiptSubmitted, ct);

        var preparing = await db.Orders.AsNoTracking().CountAsync(o => o.Status == OrderStatus.PreparingFood, ct);
        var ready = await db.Orders.AsNoTracking().CountAsync(o => o.Status == OrderStatus.Ready, ct);
        var completedToday = todaysOrders.Count(o => o.Status == OrderStatus.Completed);

        var popularProducts = (await db.OrderItems
            .AsNoTracking()
            .GroupBy(i => i.ProductNameSnapshot)
            .Select(g => new { ProductName = g.Key, TimesOrdered = g.Sum(i => i.Quantity) })
            .OrderByDescending(p => p.TimesOrdered)
            .Take(5)
            .ToListAsync(ct))
            .Select(p => new PopularProductStatDto(p.ProductName, p.TimesOrdered))
            .ToList();

        return new AdminOverviewDto(
            todaysOrders.Count,
            todaysOrders.Sum(o => o.Total),
            pendingReceipts,
            preparing,
            ready,
            completedToday,
            popularProducts);
    }
}
