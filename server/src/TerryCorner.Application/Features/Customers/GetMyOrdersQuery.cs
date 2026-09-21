using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.Customers;

public record MyOrderListItemDto(
    string OrderNumber, string Status, string PaymentStatus, decimal Total, DateTime CreatedAtUtc);

public record GetMyOrdersQuery : IRequest<IReadOnlyList<MyOrderListItemDto>>;

public class GetMyOrdersQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetMyOrdersQuery, IReadOnlyList<MyOrderListItemDto>>
{
    public async Task<IReadOnlyList<MyOrderListItemDto>> Handle(GetMyOrdersQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new NoCustomerProfileException();

        var orders = await db.Orders
            .AsNoTracking()
            .Where(o => o.Customer.ApplicationUserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(ct);

        return orders
            .Select(o => new MyOrderListItemDto(
                o.OrderNumber, o.Status.ToString(), o.PaymentStatus.ToString(), o.Total, o.CreatedAtUtc))
            .ToList();
    }
}

public record MyOrderItemForReorderDto(Guid ProductId, string ProductName, int Quantity, IReadOnlyList<Guid> ToppingIds);

public record MyOrderDetailDto(
    string OrderNumber, string Status, decimal Total, DateTime CreatedAtUtc,
    IReadOnlyList<MyOrderItemForReorderDto> Items);

public record GetMyOrderDetailQuery(string OrderNumber) : IRequest<MyOrderDetailDto>;

public class GetMyOrderDetailQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetMyOrderDetailQuery, MyOrderDetailDto>
{
    public async Task<MyOrderDetailDto> Handle(GetMyOrderDetailQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new NoCustomerProfileException();

        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Toppings)
            .FirstOrDefaultAsync(o => o.OrderNumber == request.OrderNumber && o.Customer.ApplicationUserId == userId, ct)
            ?? throw new KeyNotFoundException($"Order {request.OrderNumber} was not found.");

        return new MyOrderDetailDto(
            order.OrderNumber, order.Status.ToString(), order.Total, order.CreatedAtUtc,
            order.Items.Select(i => new MyOrderItemForReorderDto(
                i.ProductId, i.ProductNameSnapshot, i.Quantity,
                i.Toppings.Select(t => t.ToppingId).ToList())).ToList());
    }
}
