using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Entities;

namespace TerryCorner.Application.Features.Auth;

internal static class AuthResultFactory
{
    public static async Task<AuthResultDto> BuildAsync(
        ITokenService tokenService,
        IApplicationDbContext db,
        string userId,
        string email,
        string fullName,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken)
    {
        var tokens = tokenService.GenerateTokens(userId, email, fullName, roles);

        db.RefreshTokens.Add(new RefreshToken
        {
            ApplicationUserId = userId,
            TokenHash = tokenService.HashRefreshToken(tokens.RefreshToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(14),
        });
        await db.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.ExpiresAtUtc,
            new AuthUserDto(userId, email, fullName, roles));
    }
}
