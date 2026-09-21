using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.PaymentMethods;

public record PaymentMethodDto(
    Guid Id,
    string MethodName,
    string AccountName,
    string AccountNumberOrIdentifier,
    string? PhoneNumber,
    string Instructions);

public record GetActivePaymentMethodsQuery : IRequest<IReadOnlyList<PaymentMethodDto>>;

public class GetActivePaymentMethodsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetActivePaymentMethodsQuery, IReadOnlyList<PaymentMethodDto>>
{
    public async Task<IReadOnlyList<PaymentMethodDto>> Handle(GetActivePaymentMethodsQuery request, CancellationToken ct)
    {
        var methods = await db.PaymentMethods
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(ct);

        return methods
            .Select(m => new PaymentMethodDto(
                m.Id, m.MethodName, m.AccountName, m.AccountNumberOrIdentifier, m.PhoneNumber, m.Instructions))
            .ToList();
    }
}
