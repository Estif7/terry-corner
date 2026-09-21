using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Entities;
using TerryCorner.Domain.Enums;

namespace TerryCorner.Application.Features.PaymentReceipts;

public record UploadPaymentReceiptRequest(
    string OrderNumber,
    Stream FileContent,
    string OriginalFileName,
    string ContentType,
    string? TransactionReferenceNumber,
    string? PaymentNote);

public record PaymentReceiptDto(Guid Id, string OrderNumber, string PaymentStatus, DateTime SubmittedAtUtc);

public record UploadPaymentReceiptCommand(UploadPaymentReceiptRequest Request) : IRequest<PaymentReceiptDto>;

public class UploadPaymentReceiptCommandValidator : AbstractValidator<UploadPaymentReceiptCommand>
{
    public UploadPaymentReceiptCommandValidator()
    {
        RuleFor(x => x.Request.OrderNumber).NotEmpty();
        RuleFor(x => x.Request.OriginalFileName).NotEmpty();
        RuleFor(x => x.Request.ContentType).NotEmpty();
    }
}

public class DuplicateReceiptException : Exception
{
    public DuplicateReceiptException()
        : base("A payment receipt has already been submitted for this order and is awaiting review.") { }
}

public class UploadPaymentReceiptCommandHandler(
    IApplicationDbContext db,
    IFileStorageService fileStorage) : IRequestHandler<UploadPaymentReceiptCommand, PaymentReceiptDto>
{
    public async Task<PaymentReceiptDto> Handle(UploadPaymentReceiptCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var order = await db.Orders
            .Include(o => o.PaymentReceipts)
            .FirstOrDefaultAsync(o => o.OrderNumber == request.OrderNumber, ct)
            ?? throw new KeyNotFoundException($"Order {request.OrderNumber} was not found.");

        // Prevent duplicate submissions: only one receipt may be pending review at a time.
        if (order.PaymentReceipts.Any(r => r.Status == PaymentStatus.ReceiptSubmitted))
        {
            throw new DuplicateReceiptException();
        }

        var stored = await fileStorage.SaveReceiptAsync(
            request.FileContent, request.OriginalFileName, request.ContentType, ct);

        var receipt = new PaymentReceipt
        {
            Order = order,
            StoredFileName = stored.StoredFileName,
            OriginalFileNameForDisplay = Path.GetFileName(request.OriginalFileName),
            ContentType = request.ContentType,
            FileSizeBytes = stored.SizeBytes,
            TransactionReferenceNumber = request.TransactionReferenceNumber,
            PaymentNote = request.PaymentNote,
            Status = PaymentStatus.ReceiptSubmitted,
        };
        db.PaymentReceipts.Add(receipt);

        order.PaymentStatus = PaymentStatus.ReceiptSubmitted;
        order.Status = OrderStatus.PaymentVerification;
        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Order = order,
            Status = OrderStatus.PaymentReceiptSubmitted,
            PaymentStatus = PaymentStatus.ReceiptSubmitted,
            ChangedBy = "customer",
        });
        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Order = order,
            Status = OrderStatus.PaymentVerification,
            PaymentStatus = PaymentStatus.ReceiptSubmitted,
            ChangedBy = "system",
            Reason = "Receipt submitted; awaiting staff review.",
        });

        await db.SaveChangesAsync(ct);

        return new PaymentReceiptDto(receipt.Id, order.OrderNumber, receipt.Status.ToString(), receipt.CreatedAtUtc);
    }
}
