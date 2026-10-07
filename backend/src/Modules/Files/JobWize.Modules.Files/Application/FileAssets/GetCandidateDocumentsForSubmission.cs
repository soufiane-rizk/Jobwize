using JobWize.Modules.Files.Persistence;
using JobWize.Runtime.Contracts.Requests;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Files.Application.FileAssets;

public static class GetCandidateDocumentsForSubmission
{
    internal sealed class Handler(FilesDbContext dbContext)
        : IModuleQueryHandler<
            Contracts.Internal.FileAssets.GetCandidateDocumentsForSubmission.Query,
            Contracts.Internal.FileAssets.GetCandidateDocumentsForSubmission.Response>
    {
        public async Task<Contracts.Internal.FileAssets.GetCandidateDocumentsForSubmission.Response> HandleAsync(
            Contracts.Internal.FileAssets.GetCandidateDocumentsForSubmission.Query query,
            CancellationToken cancellationToken)
        {
            Guid[] distinctFileIds = query.FileIds.Distinct().ToArray();

            List<Contracts.Internal.FileAssets.GetCandidateDocumentsForSubmission.Item> files = await dbContext.FileAssets
                .AsNoTracking()
                .Where(file =>
                    distinctFileIds.Contains(file.Id) &&
                    file.CandidateId == query.CandidateId &&
                    file.Kind == Domain.FileAssetKind.CandidateDocument &&
                    file.ArchivedAt == null)
                .Select(file => new Contracts.Internal.FileAssets.GetCandidateDocumentsForSubmission.Item(
                    file.Id,
                    file.FileName,
                    file.ContentType,
                    file.SizeBytes))
                .ToListAsync(cancellationToken);

            return new(files);
        }
    }
}
