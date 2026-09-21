using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Enums;

namespace TerryCorner.Application.Features.Orders;

public record AdminOrderListItemDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string Status,
    string PaymentStatus,
    decimal Total,
    DateTime CreatedAtUtc);

public record GetOrdersForAdminQuery(string? Status, string? Search) : IRequest<IReadOnlyList<AdminOrderListItemDto>>;

public class GetOrdersForAdminQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetOrdersForAdminQuery, IReadOnlyList<AdminOrderListItemDto>>
{
    public async Task<IReadOnlyList<AdminOrderListItemDto>> Handle(GetOrdersForAdminQuery request, CancellationToken ct)
    {
        var query = db.Orders.Include(o => o.Customer).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(o => EF.Functions.Like(o.OrderNumber, $"%{term}%")
                || EF.Functions.Like(o.ContactFullName, $"%{term}%"));
        }

        var orders = await query.OrderByDescending(o => o.CreatedAtUtc).Take(200).ToListAsync(ct);

        return orders
            .Select(o => new AdminOrderListItemDto(
                o.Id, o.OrderNumber, o.Customer.FullName, o.Status.ToString(),
                o.PaymentStatus.ToString(), o.Total, o.CreatedAtUtc))
            .ToList();
    }
}
