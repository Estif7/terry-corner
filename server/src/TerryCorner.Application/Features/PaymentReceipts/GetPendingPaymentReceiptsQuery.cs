using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Enums;

namespace TerryCorner.Application.Features.PaymentReceipts;

public record PaymentReceiptReviewDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    decimal OrderTotal,
    string OriginalFileNameForDisplay,
    string? TransactionReferenceNumber,
    string? PaymentNote,
    DateTime SubmittedAtUtc);

public record GetPendingPaymentReceiptsQuery : IRequest<IReadOnlyList<PaymentReceiptReviewDto>>;

public class GetPendingPaymentReceiptsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPendingPaymentReceiptsQuery, IReadOnlyList<PaymentReceiptReviewDto>>
{
    public async Task<IReadOnlyList<PaymentReceiptReviewDto>> Handle(
        GetPendingPaymentReceiptsQuery request, CancellationToken ct)
    {
        var receipts = await db.PaymentReceipts
            .AsNoTracking()
            .Include(r => r.Order).ThenInclude(o => o.Customer)
            .Where(r => r.Status == PaymentStatus.ReceiptSubmitted)
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync(ct);

        return receipts
            .Select(r => new PaymentReceiptReviewDto(
                r.Id,
                r.Order.OrderNumber,
                r.Order.Customer.FullName,
                r.Order.Total,
                r.OriginalFileNameForDisplay,
                r.TransactionReferenceNumber,
                r.PaymentNote,
                r.CreatedAtUtc))
            .ToList();
    }
}
