using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Entities;
using TerryCorner.Domain.Enums;

namespace TerryCorner.Application.Features.Orders;

/// <summary>
/// Manual staff-driven transitions only. PreparingFood is reached exclusively via payment
/// receipt approval (see ApprovePaymentReceiptCommand) — never set directly through this endpoint.
/// </summary>
public record UpdateOrderStatusCommand(Guid OrderId, string NewStatus) : IRequest;

public record AddOrderNoteCommand(Guid OrderId, string Note) : IRequest;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    private static readonly HashSet<string> AllowedManualStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(OrderStatus.Ready),
        nameof(OrderStatus.Completed),
        nameof(OrderStatus.Cancelled),
    };

    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.NewStatus)
            .Must(s => AllowedManualStatuses.Contains(s))
            .WithMessage("Status must be one of: Ready, Completed, Cancelled. " +
                         "PreparingFood is reached automatically via payment approval.");
    }
}

public class UpdateOrderStatusCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IOrderNotifier orderNotifier) : IRequestHandler<UpdateOrderStatusCommand>
{
    public async Task Handle(UpdateOrderStatusCommand request, CancellationToken ct)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        var newStatus = Enum.Parse<OrderStatus>(request.NewStatus, ignoreCase: true);
        var staffId = currentUser.UserId ?? "staff";

        order.Status = newStatus;

        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Order = order,
            Status = newStatus,
            PaymentStatus = order.PaymentStatus,
            ChangedBy = staffId,
            Reason = "Manually updated by staff.",
        });

        db.AuditLogs.Add(new AuditLog
        {
            UserId = staffId,
            Action = "Order.StatusChanged",
            EntityType = "Order",
            EntityId = order.Id.ToString(),
            Details = $"Status manually set to {newStatus}.",
        });

        await db.SaveChangesAsync(ct);
        await orderNotifier.NotifyOrderStatusChangedAsync(order.OrderNumber, ct);
    }
}

public class AddOrderNoteCommandHandler(IApplicationDbContext db) : IRequestHandler<AddOrderNoteCommand>
{
    public async Task Handle(AddOrderNoteCommand request, CancellationToken ct)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        order.InternalStaffNotes = request.Note;
        order.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
