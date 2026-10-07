using JobWize.Modules.Files.Contracts.Events.FileAssets;
using JobWize.Modules.Files.Domain;
using JobWize.Modules.Files.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace JobWize.Modules.Files.Application.FileAssets;

public static class ArchiveFileAsset
{
    internal sealed record Command(Guid DocumentId) : ICommand<bool>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete(Contracts.Public.FileAssets.ArchiveFileAsset.Route, async (
                Guid documentId,
                IDispatcher dispatcher,
                CancellationToken cancellationToken) =>
            {
                Result<bool> result = await dispatcher.SendAsync(new Command(documentId), cancellationToken);
                return result.ToApiResult();
            })
            .RequireAuthorization()
            .WithName("ArchiveFileAsset")
            .WithTags("Files");
        }
    }

    internal sealed class Handler(
        IFileAssetRepository files,
        IUserContext userContext,
        IDispatcher dispatcher) : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            FileAsset? document = await files.GetByIdAsync(command.DocumentId, userContext.UserId, cancellationToken);
            if (document is null || document.IsArchived)
            {
                return Result<bool>.Failure(FilesErrors.DocumentNotFound);
            }

            document.Archive(DateTime.UtcNow);
            await files.SaveAsync(document, cancellationToken);
            await dispatcher.PublishAsync(new FileAssetArchived(document.Id, userContext.UserId), cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
