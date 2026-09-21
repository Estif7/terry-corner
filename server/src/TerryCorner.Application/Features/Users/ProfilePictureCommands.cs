using MediatR;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Application.Features.Users;

public record UploadProfilePictureRequest(string UserId, Stream FileContent, string OriginalFileName, string ContentType);

public record UploadProfilePictureCommand(UploadProfilePictureRequest Request) : IRequest;

public record DeleteProfilePictureCommand(string UserId) : IRequest;

public record GetProfilePictureFileNameQuery(string UserId) : IRequest<string?>;

public class UploadProfilePictureCommandHandler(
    IIdentityService identityService,
    IFileStorageService fileStorage) : IRequestHandler<UploadProfilePictureCommand>
{
    public async Task Handle(UploadProfilePictureCommand command, CancellationToken ct)
    {
        var request = command.Request;

        // Replace, don't accumulate — remove the old picture once the new one is safely saved.
        var previousFileName = await identityService.GetProfilePictureFileNameAsync(request.UserId, ct);

        var stored = await fileStorage.SaveProfilePictureAsync(
            request.FileContent, request.OriginalFileName, request.ContentType, ct);

        await identityService.SetProfilePictureFileNameAsync(request.UserId, stored.StoredFileName, ct);

        if (!string.IsNullOrEmpty(previousFileName))
        {
            fileStorage.DeleteProfilePicture(previousFileName);
        }
    }
}

public class DeleteProfilePictureCommandHandler(
    IIdentityService identityService,
    IFileStorageService fileStorage) : IRequestHandler<DeleteProfilePictureCommand>
{
    public async Task Handle(DeleteProfilePictureCommand command, CancellationToken ct)
    {
        var fileName = await identityService.GetProfilePictureFileNameAsync(command.UserId, ct);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        await identityService.SetProfilePictureFileNameAsync(command.UserId, null, ct);
        fileStorage.DeleteProfilePicture(fileName);
    }
}

public class GetProfilePictureFileNameQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetProfilePictureFileNameQuery, string?>
{
    public Task<string?> Handle(GetProfilePictureFileNameQuery request, CancellationToken ct) =>
        identityService.GetProfilePictureFileNameAsync(request.UserId, ct);
}
