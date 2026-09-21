using FluentValidation;
using MediatR;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.Auth;

public record LoginCommand(string Email, string Password) : IRequest<AuthResultDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Invalid email or password.") { }
}

public class LoginCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IApplicationDbContext db) : IRequestHandler<LoginCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.ValidateCredentialsAsync(request.Email, request.Password)
            ?? throw new InvalidCredentialsException();

        return await AuthResultFactory.BuildAsync(
            tokenService, db, user.UserId, user.Email, user.FullName, user.Roles, cancellationToken);
    }
}
