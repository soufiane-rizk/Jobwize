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

public static class DownloadFileAsset
{
    internal sealed record Query(Guid DocumentId) : IQuery<FileDownload>;

    internal sealed record FileDownload(Stream Content, string ContentType, string FileName);

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(Contracts.Public.FileAssets.DownloadFileAsset.Route, async (
                Guid documentId,
                IDispatcher dispatcher,
                CancellationToken cancellationToken) =>
            {
                Result<FileDownload> result = await dispatcher.SendAsync(new Query(documentId), cancellationToken);
                if (result.IsFailure)
                {
                    return result.ToApiResult();
                }

                return Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
            })
            .RequireAuthorization()
            .WithName("DownloadFileAsset")
            .WithTags("Files");
        }
    }

    internal sealed class Handler(
        IFileAssetRepository files,
        IFileStorage storage,
        IUserContext userContext) : IQueryHandler<Query, FileDownload>
    {
        public async Task<Result<FileDownload>> HandleAsync(Query query, CancellationToken cancellationToken)
        {
            Domain.FileAsset? file = await files.GetByIdAsync(query.DocumentId, userContext.UserId, cancellationToken);
            if (file is null ||
                file.Kind != Domain.FileAssetKind.CandidateDocument ||
                (file.IsArchived && !file.HasActiveBindings))
            {
                return Result<FileDownload>.Failure(FilesErrors.DocumentNotFound);
            }

            Stream? content = await storage.OpenReadAsync(file.StorageKey, cancellationToken);
            return content is null
                ? Result<FileDownload>.Failure(FilesErrors.DocumentNotFound)
                : Result<FileDownload>.Success(new(content, file.ContentType, file.FileName));
        }
    }
}
