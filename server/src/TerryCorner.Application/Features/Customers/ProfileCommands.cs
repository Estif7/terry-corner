using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.Customers;

public record MyProfileDto(string FullName, string PhoneNumber, string? Email, bool HasProfilePicture);

public record GetMyProfileQuery : IRequest<MyProfileDto>;

public record UpdateMyProfileCommand(string FullName, string PhoneNumber) : IRequest;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(30);
    }
}

public class NoCustomerProfileException : Exception
{
    public NoCustomerProfileException() : base("No customer profile was found for this account.") { }
}

public class GetMyProfileQueryHandler(
    IApplicationDbContext db, ICurrentUserService currentUser, IIdentityService identityService)
    : IRequestHandler<GetMyProfileQuery, MyProfileDto>
{
    public async Task<MyProfileDto> Handle(GetMyProfileQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new NoCustomerProfileException();

        var customer = await db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ApplicationUserId == userId, ct)
            ?? throw new NoCustomerProfileException();

        var pictureFileName = await identityService.GetProfilePictureFileNameAsync(userId, ct);

        return new MyProfileDto(
            customer.FullName, customer.PhoneNumber, customer.Email, !string.IsNullOrEmpty(pictureFileName));
    }
}

public class UpdateMyProfileCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<UpdateMyProfileCommand>
{
    public async Task Handle(UpdateMyProfileCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new NoCustomerProfileException();

        var customer = await db.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId, ct)
            ?? throw new NoCustomerProfileException();

        customer.FullName = request.FullName;
        customer.PhoneNumber = request.PhoneNumber;
        customer.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
