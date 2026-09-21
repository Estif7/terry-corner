using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.Orders;

public record AdminOrderReceiptDto(Guid Id, string Status, string? TransactionReferenceNumber, DateTime SubmittedAtUtc);

public record AdminOrderDetailDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string ContactPhoneNumber,
    string Status,
    string PaymentStatus,
    string OrderType,
    string? DeliveryAddress,
    string? OrderNotes,
    string? InternalStaffNotes,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<AdminOrderReceiptDto> Receipts,
    DateTime CreatedAtUtc);

public record GetOrderDetailForAdminQuery(Guid OrderId) : IRequest<AdminOrderDetailDto>;

public class GetOrderDetailForAdminQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetOrderDetailForAdminQuery, AdminOrderDetailDto>
{
    public async Task<AdminOrderDetailDto> Handle(GetOrderDetailForAdminQuery request, CancellationToken ct)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Toppings)
            .Include(o => o.PaymentReceipts)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        return new AdminOrderDetailDto(
            order.Id,
            order.OrderNumber,
            order.Customer.FullName,
            order.ContactPhoneNumber,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.OrderType.ToString(),
            order.DeliveryAddress,
            order.OrderNotes,
            order.InternalStaffNotes,
            order.Subtotal,
            order.DiscountAmount,
            order.Total,
            order.Items.Select(i => new OrderItemDto(
                i.ProductId, i.ProductNameSnapshot, i.UnitPriceSnapshot, i.Quantity,
                i.Toppings.Select(t => t.ToppingNameSnapshot).ToList(), i.LineTotal)).ToList(),
            order.PaymentReceipts.Select(r => new AdminOrderReceiptDto(
                r.Id, r.Status.ToString(), r.TransactionReferenceNumber, r.CreatedAtUtc)).ToList(),
            order.CreatedAtUtc);
    }
}
