using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Entities;

namespace TerryCorner.Application.Features.PaymentMethods;

public record CreatePaymentMethodCommand(
    string MethodName,
    string AccountName,
    string AccountNumberOrIdentifier,
    string? PhoneNumber,
    string Instructions,
    int SortOrder) : IRequest<PaymentMethodDto>;

public record UpdatePaymentMethodCommand(
    Guid Id,
    string MethodName,
    string AccountName,
    string AccountNumberOrIdentifier,
    string? PhoneNumber,
    string Instructions,
    int SortOrder,
    bool IsActive) : IRequest;

public record DeletePaymentMethodCommand(Guid Id) : IRequest;

public class CreatePaymentMethodCommandValidator : AbstractValidator<CreatePaymentMethodCommand>
{
    public CreatePaymentMethodCommandValidator()
    {
        RuleFor(x => x.MethodName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AccountName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.AccountNumberOrIdentifier).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Instructions).NotEmpty();
    }
}

public class UpdatePaymentMethodCommandValidator : AbstractValidator<UpdatePaymentMethodCommand>
{
    public UpdatePaymentMethodCommandValidator()
    {
        RuleFor(x => x.MethodName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AccountName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.AccountNumberOrIdentifier).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Instructions).NotEmpty();
    }
}

public class CreatePaymentMethodCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreatePaymentMethodCommand, PaymentMethodDto>
{
    public async Task<PaymentMethodDto> Handle(CreatePaymentMethodCommand request, CancellationToken ct)
    {
        var method = new PaymentMethod
        {
            MethodName = request.MethodName,
            AccountName = request.AccountName,
            AccountNumberOrIdentifier = request.AccountNumberOrIdentifier,
            PhoneNumber = request.PhoneNumber,
            Instructions = request.Instructions,
            SortOrder = request.SortOrder,
        };
        db.PaymentMethods.Add(method);
        await db.SaveChangesAsync(ct);

        return new PaymentMethodDto(
            method.Id, method.MethodName, method.AccountName, method.AccountNumberOrIdentifier,
            method.PhoneNumber, method.Instructions);
    }
}

public class UpdatePaymentMethodCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdatePaymentMethodCommand>
{
    public async Task Handle(UpdatePaymentMethodCommand request, CancellationToken ct)
    {
        var method = await db.PaymentMethods.FirstOrDefaultAsync(m => m.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Payment method {request.Id} was not found.");

        method.MethodName = request.MethodName;
        method.AccountName = request.AccountName;
        method.AccountNumberOrIdentifier = request.AccountNumberOrIdentifier;
        method.PhoneNumber = request.PhoneNumber;
        method.Instructions = request.Instructions;
        method.SortOrder = request.SortOrder;
        method.IsActive = request.IsActive;
        method.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}

public class DeletePaymentMethodCommandHandler(IApplicationDbContext db) : IRequestHandler<DeletePaymentMethodCommand>
{
    public async Task Handle(DeletePaymentMethodCommand request, CancellationToken ct)
    {
        var method = await db.PaymentMethods.FirstOrDefaultAsync(m => m.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Payment method {request.Id} was not found.");

        db.PaymentMethods.Remove(method);
        await db.SaveChangesAsync(ct);
    }
}
