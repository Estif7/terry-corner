using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.Auth;

public record LogoutCommand(string RefreshToken) : IRequest;

public class LogoutCommandHandler(
    ITokenService tokenService,
    IApplicationDbContext db) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (stored is not null && stored.RevokedAtUtc is null)
        {
            stored.RevokedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
