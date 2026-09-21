using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Entities;
using TerryCorner.Domain.Enums;

namespace TerryCorner.Application.Features.PaymentReceipts;

public record ApprovePaymentReceiptCommand(Guid ReceiptId) : IRequest;

public class ApprovePaymentReceiptCommandValidator : AbstractValidator<ApprovePaymentReceiptCommand>
{
    public ApprovePaymentReceiptCommandValidator()
    {
        RuleFor(x => x.ReceiptId).NotEmpty();
    }
}

public class ReceiptAlreadyReviewedException : Exception
{
    public ReceiptAlreadyReviewedException()
        : base("This receipt has already been reviewed.") { }
}

public class ApprovePaymentReceiptCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IOrderNotifier orderNotifier) : IRequestHandler<ApprovePaymentReceiptCommand>
{
    public async Task Handle(ApprovePaymentReceiptCommand command, CancellationToken ct)
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

        // 1. Mark the payment as Verified.
        receipt.Status = PaymentStatus.Verified;
        receipt.ReviewedByUserId = reviewerId;
        receipt.ReviewedAtUtc = DateTime.UtcNow;

        // 2. Mark the order as PreparingFood — this is the critical automatic transition:
        //    staff only approve the receipt once; they never touch order status separately.
        var order = receipt.Order;
        order.PaymentStatus = PaymentStatus.Verified;
        order.Status = OrderStatus.PreparingFood;

        // 3. Record the status change in order history for audit purposes.
        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Order = order,
            Status = OrderStatus.PreparingFood,
            PaymentStatus = PaymentStatus.Verified,
            ChangedBy = reviewerId,
            Reason = "Payment receipt approved by staff.",
        });

        db.AuditLogs.Add(new AuditLog
        {
            UserId = reviewerId,
            Action = "PaymentReceipt.Approved",
            EntityType = "Order",
            EntityId = order.Id.ToString(),
            Details = $"Receipt {receipt.Id} approved; order moved to PreparingFood.",
        });

        await db.SaveChangesAsync(ct);

        // 4/5. Notify the customer's tracking page in real time.
        await orderNotifier.NotifyOrderStatusChangedAsync(order.OrderNumber, ct);
    }
}
