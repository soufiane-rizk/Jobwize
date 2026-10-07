using JobWize.Modules.Files.Domain;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Files.Persistence;

public interface IFileAssetRepository
{
    Task<FileAsset?> GetByIdAsync(Guid documentId, Guid candidateId, CancellationToken cancellationToken = default);
    Task SaveAsync(FileAsset document, CancellationToken cancellationToken = default);
}

internal sealed class FileAssetRepository(FilesDbContext dbContext) : IFileAssetRepository
{
    public Task<FileAsset?> GetByIdAsync(Guid documentId, Guid candidateId, CancellationToken cancellationToken = default) =>
        dbContext.FileAssets.SingleOrDefaultAsync(
            document => document.Id == documentId && document.CandidateId == candidateId,
            cancellationToken);

    public Task SaveAsync(FileAsset document, CancellationToken cancellationToken = default)
    {
        if (dbContext.Entry(document).State == EntityState.Detached)
        {
            dbContext.FileAssets.Add(document);
        }

        return Task.CompletedTask;
    }
}
