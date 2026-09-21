using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.PaymentReceipts;

public record GetPaymentReceiptStoredFileNameQuery(Guid ReceiptId) : IRequest<string>;

public class GetPaymentReceiptStoredFileNameQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPaymentReceiptStoredFileNameQuery, string>
{
    public async Task<string> Handle(GetPaymentReceiptStoredFileNameQuery request, CancellationToken ct)
    {
        var storedFileName = await db.PaymentReceipts
            .AsNoTracking()
            .Where(r => r.Id == request.ReceiptId)
            .Select(r => r.StoredFileName)
            .FirstOrDefaultAsync(ct);

        return storedFileName ?? throw new KeyNotFoundException($"Payment receipt {request.ReceiptId} was not found.");
    }
}
