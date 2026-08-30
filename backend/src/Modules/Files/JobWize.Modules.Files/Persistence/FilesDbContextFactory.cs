using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JobWize.Modules.Files.Persistence;

internal sealed class FilesDbContextFactory : IDesignTimeDbContextFactory<FilesDbContext>
{
    public FilesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FilesDbContext>()
            .UseNpgsql("Host=localhost;Database=jobwize;Username=jobwize;Password=jobwize")
            .Options;

        return new FilesDbContext(options);
    }
}
