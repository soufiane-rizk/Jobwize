using JobWize.Modules.Files.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Files.Application.FileAssets;

public static class GetFileAssets
{
    internal sealed record Query : IQuery<Contracts.Public.FileAssets.GetFileAssets.Response>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(Contracts.Public.FileAssets.GetFileAssets.Route, async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
            {
                Result<Contracts.Public.FileAssets.GetFileAssets.Response> result =
                    await dispatcher.SendAsync(new Query(), cancellationToken);
                return result.ToApiResult();
            })
            .RequireAuthorization()
            .WithName("GetFileAssets")
            .WithTags("Files");
        }
    }

    internal sealed class Handler(FilesDbContext dbContext, IUserContext userContext)
        : IQueryHandler<Query, Contracts.Public.FileAssets.GetFileAssets.Response>
    {
        public async Task<Result<Contracts.Public.FileAssets.GetFileAssets.Response>> HandleAsync(Query query, CancellationToken cancellationToken)
        {
            List<Contracts.Public.FileAssets.GetFileAssets.Item> files = await dbContext.FileAssets
                .AsNoTracking()
                .Where(document =>
                    document.CandidateId == userContext.UserId &&
                    document.Kind == Domain.FileAssetKind.CandidateDocument &&
                    document.ArchivedAt == null)
                .OrderByDescending(document => document.UploadedAt)
                .Select(document => new Contracts.Public.FileAssets.GetFileAssets.Item(
                    document.Id,
                    document.FileName,
                    document.ContentType,
                    document.SizeBytes,
                    document.UploadedAt))
                .ToListAsync(cancellationToken);

            return Result<Contracts.Public.FileAssets.GetFileAssets.Response>.Success(new(files));
        }
    }
}
