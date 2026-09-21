namespace TerryCorner.Application.Common.Interfaces;

/// <summary>Read-only view of the authenticated caller, populated per-request by Infrastructure/Api.</summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);
}

public record TokenPair(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);

public interface ITokenService
{
    TokenPair GenerateTokens(string userId, string email, string fullName, IEnumerable<string> roles);
    string HashRefreshToken(string rawRefreshToken);
}
