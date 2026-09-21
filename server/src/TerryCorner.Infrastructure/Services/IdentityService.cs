using Microsoft.AspNetCore.Identity;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Infrastructure.Identity;
using AppIdentityResult = TerryCorner.Application.Common.Interfaces.IdentityResult;

namespace TerryCorner.Infrastructure.Services;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : IIdentityService
{
    public async Task<AppIdentityResult> CreateUserAsync(string email, string password, string fullName, string role)
    {
        await EnsureRoleExistsAsync(role);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            CreatedAtUtc = DateTime.UtcNow,
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return new AppIdentityResult(false, null, createResult.Errors.Select(e => e.Description).ToList());
        }

        await userManager.AddToRoleAsync(user, role);
        return new AppIdentityResult(true, user.Id, Array.Empty<string>());
    }

    public async Task<ValidatedUser?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return null;
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
        {
            await userManager.AccessFailedAsync(user);
            return null;
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        return new ValidatedUser(user.Id, user.Email!, user.FullName, roles.ToList());
    }

    public async Task<ValidatedUser?> GetUserByIdAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return new ValidatedUser(user.Id, user.Email!, user.FullName, roles.ToList());
    }

    // ---- Admin user management --------------------------------------------------------

    public async Task<IReadOnlyList<AdminUserDto>> GetAllUsersAsync(CancellationToken ct)
    {
        var users = userManager.Users.ToList();
        var result = new List<AdminUserDto>(users.Count);

        foreach (var user in users)
        {
            result.Add(await ToAdminUserDtoAsync(user));
        }

        return result;
    }

    public async Task<AdminUserDto?> GetUserForAdminAsync(string userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user is null ? null : await ToAdminUserDtoAsync(user);
    }

    public async Task<AppIdentityResult> UpdateUserProfileAsync(
        string userId, string fullName, string email, string? phoneNumber, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new AppIdentityResult(false, null, new[] { "User not found." });
        }

        user.FullName = fullName;
        user.PhoneNumber = phoneNumber;

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var emailResult = await userManager.SetEmailAsync(user, email);
            if (!emailResult.Succeeded)
            {
                return new AppIdentityResult(false, null, emailResult.Errors.Select(e => e.Description).ToList());
            }
            // Username mirrors email in this app (see CreateUserAsync) — keep them in sync.
            await userManager.SetUserNameAsync(user, email);
        }

        var updateResult = await userManager.UpdateAsync(user);
        return updateResult.Succeeded
            ? new AppIdentityResult(true, user.Id, Array.Empty<string>())
            : new AppIdentityResult(false, null, updateResult.Errors.Select(e => e.Description).ToList());
    }

    public async Task SetUserLockoutAsync(string userId, bool locked, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return;
        }

        if (!user.LockoutEnabled)
        {
            await userManager.SetLockoutEnabledAsync(user, true);
        }

        await userManager.SetLockoutEndDateAsync(user, locked ? DateTimeOffset.MaxValue : null);
    }

    public async Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct)
    {
        var totalUsers = userManager.Users.Count();
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var newUsersLast7Days = userManager.Users.Count(u => u.CreatedAtUtc != null && u.CreatedAtUtc >= sevenDaysAgo);
        var lockedOutCount = userManager.Users.Count(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);

        var customerCount = (await userManager.GetUsersInRoleAsync(Roles.Customer)).Count;
        var staffCount = (await userManager.GetUsersInRoleAsync(Roles.Staff)).Count;
        var managerCount = (await userManager.GetUsersInRoleAsync(Roles.Manager)).Count;
        var adminCount = (await userManager.GetUsersInRoleAsync(Roles.Admin)).Count;

        return new UserStatsDto(
            totalUsers, customerCount, staffCount, managerCount, adminCount, lockedOutCount, newUsersLast7Days);
    }

    public async Task ChangeUserRoleAsync(string userId, string newRole, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return;
        }

        await EnsureRoleExistsAsync(newRole);

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await userManager.AddToRoleAsync(user, newRole);
    }

    // ---- Profile pictures ---------------------------------------------------------------

    public async Task<string?> GetProfilePictureFileNameAsync(string userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user?.ProfilePictureFileName;
    }

    public async Task SetProfilePictureFileNameAsync(string userId, string? storedFileName, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return;
        }

        user.ProfilePictureFileName = storedFileName;
        await userManager.UpdateAsync(user);
    }

    private async Task<AdminUserDto> ToAdminUserDtoAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var isLockedOut = await userManager.IsLockedOutAsync(user);

        return new AdminUserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            user.PhoneNumber,
            roles.ToList(),
            isLockedOut,
            !string.IsNullOrEmpty(user.ProfilePictureFileName),
            user.CreatedAtUtc);
    }

    private async Task EnsureRoleExistsAsync(string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
