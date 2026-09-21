using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Entities;
using TerryCorner.Domain.Enums;

namespace TerryCorner.Application.Features.PaymentReceipts;

public record RejectPaymentReceiptCommand(Guid ReceiptId, string Reason) : IRequest;

public class RejectPaymentReceiptCommandValidator : AbstractValidator<RejectPaymentReceiptCommand>
{
    public RejectPaymentReceiptCommandValidator()
    {
        RuleFor(x => x.ReceiptId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class RejectPaymentReceiptCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IOrderNotifier orderNotifier) : IRequestHandler<RejectPaymentReceiptCommand>
{
    public async Task Handle(RejectPaymentReceiptCommand command, CancellationToken ct)
    {
        var receipt = await db.PaymentReceipts
            .Include(r => r.Order)
            .FirstOrDefaultAsync(r => r.Id == command.ReceiptId, ct)
            ?? throw new KeyNotFoundException($"Payment receipt {command.ReceiptId} was not found.");

        if (receipt.Status != PaymentStatus.ReceiptSubmitted)
        {
            throw new ReceiptAlreadyReviewedException();
        }

        var reviewerId = currentUser.UserId ?? "staff";

        receipt.Status = PaymentStatus.Rejected;
        receipt.RejectionReason = command.Reason;
        receipt.ReviewedByUserId = reviewerId;
        receipt.ReviewedAtUtc = DateTime.UtcNow;

        // Send the order back to PaymentPending so the customer can submit a new receipt —
        // the upload endpoint only blocks a second submission while one is ReceiptSubmitted.
        var order = receipt.Order;
        order.PaymentStatus = PaymentStatus.Rejected;
        order.Status = OrderStatus.PaymentPending;

        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Order = order,
            Status = OrderStatus.PaymentPending,
            PaymentStatus = PaymentStatus.Rejected,
            ChangedBy = reviewerId,
            Reason = $"Payment receipt rejected: {command.Reason}",
        });

        db.AuditLogs.Add(new AuditLog
        {
            UserId = reviewerId,
            Action = "PaymentReceipt.Rejected",
            EntityType = "Order",
            EntityId = order.Id.ToString(),
            Details = $"Receipt {receipt.Id} rejected: {command.Reason}",
        });

        await db.SaveChangesAsync(ct);

        await orderNotifier.NotifyOrderStatusChangedAsync(order.OrderNumber, ct);
    }
}
