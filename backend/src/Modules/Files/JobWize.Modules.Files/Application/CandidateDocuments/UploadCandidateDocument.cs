using JobWize.Modules.Files.Contracts.Events.FileAssets;
using JobWize.Modules.Files.Domain;
using JobWize.Modules.Files.Persistence;
using JobWize.Modules.Files.Storage;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace JobWize.Modules.Files.Application.FileAssets;

public static class UploadFileAsset
{
    internal sealed record Command(string FileName, string? DeclaredContentType, byte[] Content)
        : ICommand<Contracts.Public.FileAssets.UploadFileAsset.Response>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(Contracts.Public.FileAssets.UploadFileAsset.Route, async (
                IFormFile? file,
                IDispatcher dispatcher,
                CancellationToken cancellationToken) =>
            {
                if (file is null || file.Length is <= 0 or > DocumentValidation.MaximumSizeBytes)
                {
                    return Results.BadRequest(FilesErrors.InvalidFile);
                }

                await using var content = new MemoryStream();
                await file.CopyToAsync(content, cancellationToken);

                Result<Contracts.Public.FileAssets.UploadFileAsset.Response> result =
                    await dispatcher.SendAsync(new Command(file.FileName, file.ContentType, content.ToArray()), cancellationToken);

                return result.ToApiResult();
            })
            .RequireAuthorization()
            .WithName("UploadFileAsset")
            .WithTags("Files")
            .DisableAntiforgery();
        }
    }

    internal sealed class Handler(
        IFileAssetRepository files,
        IFileStorage storage,
        IUserContext userContext,
        IDispatcher dispatcher) : ICommandHandler<Command, Contracts.Public.FileAssets.UploadFileAsset.Response>
    {
        public async Task<Result<Contracts.Public.FileAssets.UploadFileAsset.Response>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            if (command.Content.Length <= 0 || command.Content.Length > DocumentValidation.MaximumSizeBytes ||
                !DocumentValidation.IsSupported(command.FileName, command.DeclaredContentType, command.Content, out string contentType))
            {
                return Result<Contracts.Public.FileAssets.UploadFileAsset.Response>.Failure(FilesErrors.InvalidFile);
            }

            Guid documentId = Guid.NewGuid();
            string storageKey = $"{userContext.UserId:N}/{documentId:N}";
            FileAsset document = FileAsset.Create(
                documentId,
                userContext.UserId,
                FileAssetKind.CandidateDocument,
                Path.GetFileName(command.FileName),
                contentType,
                command.Content.LongLength,
                storageKey);

            await using var content = new MemoryStream(command.Content, writable: false);
            await storage.StoreAsync(storageKey, content, cancellationToken);
            await files.SaveAsync(document, cancellationToken);

            await dispatcher.PublishAsync(new FileAssetUploaded(document.Id, userContext.UserId), cancellationToken);

            return Result<Contracts.Public.FileAssets.UploadFileAsset.Response>.Success(new(document.Id));
        }
    }
}
