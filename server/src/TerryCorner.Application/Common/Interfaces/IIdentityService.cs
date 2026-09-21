namespace TerryCorner.Application.Common.Interfaces;

public record IdentityResult(bool Succeeded, string? UserId, IReadOnlyList<string> Errors);

public record ValidatedUser(string UserId, string Email, string FullName, IReadOnlyList<string> Roles);

public record AdminUserDto(
    string Id,
    string Email,
    string FullName,
    string? PhoneNumber,
    IReadOnlyList<string> Roles,
    bool IsLockedOut,
    bool HasProfilePicture,
    DateTime? CreatedAtUtc);

public record UserStatsDto(
    int TotalUsers,
    int CustomerCount,
    int StaffCount,
    int ManagerCount,
    int AdminCount,
    int LockedOutCount,
    int NewUsersLast7Days);

/// <summary>
/// Wraps ASP.NET Core Identity so the Application layer never references
/// UserManager/ApplicationUser directly. Implemented in Infrastructure.
/// </summary>
public interface IIdentityService
{
    Task<IdentityResult> CreateUserAsync(string email, string password, string fullName, string role);

    /// <summary>Returns null if the email/password combination is invalid or the account is locked out.</summary>
    Task<ValidatedUser?> ValidateCredentialsAsync(string email, string password);

    Task<ValidatedUser?> GetUserByIdAsync(string userId);

    // ---- Admin user management --------------------------------------------------------
    Task<IReadOnlyList<AdminUserDto>> GetAllUsersAsync(CancellationToken ct);

    Task<AdminUserDto?> GetUserForAdminAsync(string userId, CancellationToken ct);

    Task<IdentityResult> UpdateUserProfileAsync(
        string userId, string fullName, string email, string? phoneNumber, CancellationToken ct);

    /// <summary>Indefinitely locks (or clears the lock on) sign-in for this account.</summary>
    Task SetUserLockoutAsync(string userId, bool locked, CancellationToken ct);

    Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct);

    /// <summary>Replaces all of the user's current roles with a single new one.</summary>
    Task ChangeUserRoleAsync(string userId, string newRole, CancellationToken ct);

    // ---- Profile pictures ---------------------------------------------------------------
    Task<string?> GetProfilePictureFileNameAsync(string userId, CancellationToken ct);

    Task SetProfilePictureFileNameAsync(string userId, string? storedFileName, CancellationToken ct);
}
