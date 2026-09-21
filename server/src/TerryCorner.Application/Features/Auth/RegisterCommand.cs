using FluentValidation;
using MediatR;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Entities;

namespace TerryCorner.Application.Features.Auth;

public record RegisterCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password) : IRequest<AuthResultDto>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(10);
    }
}

public class RegisterCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IApplicationDbContext db) : IRequestHandler<RegisterCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.CreateUserAsync(
            request.Email, request.Password, request.FullName, role: "Customer");

        if (!result.Succeeded || result.UserId is null)
        {
            throw new ValidationException(
                result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(nameof(request.Email), e)));
        }

        // Every registered user gets a linked Customer profile for order history/reordering.
        var customer = new Customer
        {
            ApplicationUserId = result.UserId,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);

        return await AuthResultFactory.BuildAsync(
            tokenService, db, result.UserId, request.Email, request.FullName, new[] { "Customer" }, cancellationToken);
    }
}
