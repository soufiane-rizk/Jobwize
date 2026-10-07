using JobWize.Modules.Files.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Files.Persistence;

public sealed class FilesDbContext(DbContextOptions<FilesDbContext> options) : ModuleDbContext(options)
{
    public DbSet<FileAsset> FileAssets => Set<FileAsset>();
    public DbSet<FileBinding> FileBindings => Set<FileBinding>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FilesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
