using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Entities;

namespace TerryCorner.Application.Features.Auth;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResultDto>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class InvalidRefreshTokenException : Exception
{
    public InvalidRefreshTokenException() : base("The refresh token is invalid or has expired.") { }
}

public class RefreshTokenCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IApplicationDbContext db) : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var incomingHash = tokenService.HashRefreshToken(request.RefreshToken);

        var stored = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == incomingHash, cancellationToken);

        if (stored is null)
        {
            throw new InvalidRefreshTokenException();
        }

        if (!stored.IsActive)
        {
            // Reuse of an already-rotated or expired token: treat as a potential theft and
            // revoke every active token for this user, forcing re-authentication everywhere.
            var allUserTokens = await db.RefreshTokens
                .Where(t => t.ApplicationUserId == stored.ApplicationUserId && t.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);
            foreach (var token in allUserTokens)
            {
                token.RevokedAtUtc = DateTime.UtcNow;
            }
            await db.SaveChangesAsync(cancellationToken);
            throw new InvalidRefreshTokenException();
        }

        var user = await identityService.GetUserByIdAsync(stored.ApplicationUserId)
            ?? throw new InvalidRefreshTokenException();

        var newTokens = tokenService.GenerateTokens(user.UserId, user.Email, user.FullName, user.Roles);
        var newHash = tokenService.HashRefreshToken(newTokens.RefreshToken);

        stored.RevokedAtUtc = DateTime.UtcNow;
        stored.ReplacedByTokenHash = newHash;

        db.RefreshTokens.Add(new RefreshToken
        {
            ApplicationUserId = user.UserId,
            TokenHash = newHash,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(14),
        });
        await db.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(
            newTokens.AccessToken,
            newTokens.RefreshToken,
            newTokens.ExpiresAtUtc,
            new AuthUserDto(user.UserId, user.Email, user.FullName, user.Roles));
    }
}
