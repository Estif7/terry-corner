using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.PaymentMethods;

public record AdminPaymentMethodDto(
    Guid Id,
    string MethodName,
    string AccountName,
    string AccountNumberOrIdentifier,
    string? PhoneNumber,
    string Instructions,
    int SortOrder,
    bool IsActive);

public record GetAllPaymentMethodsQuery : IRequest<IReadOnlyList<AdminPaymentMethodDto>>;

public class GetAllPaymentMethodsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetAllPaymentMethodsQuery, IReadOnlyList<AdminPaymentMethodDto>>
{
    public async Task<IReadOnlyList<AdminPaymentMethodDto>> Handle(GetAllPaymentMethodsQuery request, CancellationToken ct)
    {
        var methods = await db.PaymentMethods.AsNoTracking().OrderBy(m => m.SortOrder).ToListAsync(ct);

        return methods
            .Select(m => new AdminPaymentMethodDto(
                m.Id, m.MethodName, m.AccountName, m.AccountNumberOrIdentifier,
                m.PhoneNumber, m.Instructions, m.SortOrder, m.IsActive))
            .ToList();
    }
}
