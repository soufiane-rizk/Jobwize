using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobWize.Modules.Identity.Infrastructure;

internal sealed class InitialSuperAdminBootstrapHostedService(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<InitialSuperAdminOptions> options,
    ILogger<InitialSuperAdminBootstrapHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        InitialSuperAdminOptions bootstrapOptions = options.Value;
        if (string.IsNullOrWhiteSpace(bootstrapOptions.Email) && string.IsNullOrWhiteSpace(bootstrapOptions.TemporaryPassword))
        {
            logger.LogInformation("Initial SuperAdmin bootstrap is not configured.");
            return;
        }

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        var bootstrapper = scope.ServiceProvider.GetRequiredService<InitialSuperAdminBootstrapper>();
        bool created = await bootstrapper.BootstrapAsync(bootstrapOptions, cancellationToken);

        logger.LogInformation(
            created ? "Initial SuperAdmin account was created." : "Initial SuperAdmin account already exists; bootstrap skipped.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
