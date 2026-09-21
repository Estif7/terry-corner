namespace TerryCorner.Application.Features.Auth;

public record AuthUserDto(string Id, string Email, string FullName, IReadOnlyList<string> Roles);

public record AuthResultDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    AuthUserDto User);
