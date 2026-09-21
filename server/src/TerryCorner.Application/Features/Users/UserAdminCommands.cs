using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Entities;

namespace TerryCorner.Application.Features.Users;

public record GetAllUsersQuery : IRequest<IReadOnlyList<AdminUserDto>>;

public class GetAllUsersQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetAllUsersQuery, IReadOnlyList<AdminUserDto>>
{
    public Task<IReadOnlyList<AdminUserDto>> Handle(GetAllUsersQuery request, CancellationToken ct) =>
        identityService.GetAllUsersAsync(ct);
}

public record GetUserStatsQuery : IRequest<UserStatsDto>;

public class GetUserStatsQueryHandler(IIdentityService identityService) : IRequestHandler<GetUserStatsQuery, UserStatsDto>
{
    public Task<UserStatsDto> Handle(GetUserStatsQuery request, CancellationToken ct) =>
        identityService.GetUserStatsAsync(ct);
}

public record UpdateUserCommand(string UserId, string FullName, string Email, string? PhoneNumber) : IRequest;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}

public class UserNotFoundException : Exception
{
    public UserNotFoundException() : base("User not found.") { }
}

public class UpdateUserCommandHandler(IIdentityService identityService) : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var result = await identityService.UpdateUserProfileAsync(
            request.UserId, request.FullName, request.Email, request.PhoneNumber, ct);

        if (!result.Succeeded)
        {
            throw new ValidationException(
                result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(nameof(request.Email), e)));
        }
    }
}

public record CreateManagerAccountCommand(string FullName, string Email, string Password) : IRequest<AdminUserDto>;

public class CreateManagerAccountCommandValidator : AbstractValidator<CreateManagerAccountCommand>
{
    public CreateManagerAccountCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(10);
    }
}

public class CreateManagerAccountCommandHandler(IIdentityService identityService)
    : IRequestHandler<CreateManagerAccountCommand, AdminUserDto>
{
    public async Task<AdminUserDto> Handle(CreateManagerAccountCommand request, CancellationToken ct)
    {
        // Manager is the only role this endpoint can create — never Admin, and never routed
        // through the public /api/auth/register path (which always creates Customer accounts).
        var result = await identityService.CreateUserAsync(request.Email, request.Password, request.FullName, "Manager");

        if (!result.Succeeded || result.UserId is null)
        {
            throw new ValidationException(
                result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(nameof(request.Email), e)));
        }

        return await identityService.GetUserForAdminAsync(result.UserId, ct)
            ?? throw new UserNotFoundException();
    }
}

public record ChangeUserRoleCommand(string UserId, string NewRole) : IRequest;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Customer", "Manager", "Admin",
    };

    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewRole).Must(r => AllowedRoles.Contains(r))
            .WithMessage("Role must be one of: Customer, Manager, Admin.");
    }
}

public class CannotChangeOwnRoleException : Exception
{
    public CannotChangeOwnRoleException() : base("You can't change your own role.") { }
}

public class ChangeUserRoleCommandHandler(
    IIdentityService identityService,
    ICurrentUserService currentUser,
    IApplicationDbContext db) : IRequestHandler<ChangeUserRoleCommand>
{
    public async Task Handle(ChangeUserRoleCommand request, CancellationToken ct)
    {
        if (currentUser.UserId == request.UserId)
        {
            throw new CannotChangeOwnRoleException();
        }

        await identityService.ChangeUserRoleAsync(request.UserId, request.NewRole, ct);

        db.AuditLogs.Add(new AuditLog
        {
            UserId = currentUser.UserId ?? "system",
            Action = "User.RoleChanged",
            EntityType = "ApplicationUser",
            EntityId = request.UserId,
            Details = $"Role changed to {request.NewRole}.",
        });

        await db.SaveChangesAsync(ct);
    }
}

public record RestrictUserCommand(string UserId) : IRequest;
public record UnrestrictUserCommand(string UserId) : IRequest;

public class CannotRestrictSelfException : Exception
{
    public CannotRestrictSelfException() : base("You can't restrict your own account.") { }
}

public class RestrictUserCommandHandler(
    IIdentityService identityService,
    ICurrentUserService currentUser,
    IApplicationDbContext db) : IRequestHandler<RestrictUserCommand>
{
    public async Task Handle(RestrictUserCommand request, CancellationToken ct)
    {
        if (currentUser.UserId == request.UserId)
        {
            throw new CannotRestrictSelfException();
        }

        await identityService.SetUserLockoutAsync(request.UserId, locked: true, ct);

        // Immediately invalidate any active sessions, not just future logins.
        var activeTokens = await db.RefreshTokens
            .Where(t => t.ApplicationUserId == request.UserId && t.RevokedAtUtc == null)
            .ToListAsync(ct);
        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
        }

        db.AuditLogs.Add(new AuditLog
        {
            UserId = currentUser.UserId ?? "system",
            Action = "User.Restricted",
            EntityType = "ApplicationUser",
            EntityId = request.UserId,
        });

        await db.SaveChangesAsync(ct);
    }
}

public class UnrestrictUserCommandHandler(
    IIdentityService identityService,
    ICurrentUserService currentUser,
    IApplicationDbContext db) : IRequestHandler<UnrestrictUserCommand>
{
    public async Task Handle(UnrestrictUserCommand request, CancellationToken ct)
    {
        await identityService.SetUserLockoutAsync(request.UserId, locked: false, ct);

        db.AuditLogs.Add(new AuditLog
        {
            UserId = currentUser.UserId ?? "system",
            Action = "User.Unrestricted",
            EntityType = "ApplicationUser",
            EntityId = request.UserId,
        });

        await db.SaveChangesAsync(ct);
    }
}
