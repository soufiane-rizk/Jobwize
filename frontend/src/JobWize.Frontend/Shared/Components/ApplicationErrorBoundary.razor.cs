using Microsoft.AspNetCore.Components;

namespace JobWize.Frontend.Shared.Components
{
    public partial class ApplicationErrorBoundary : ErrorBoundaryBase
    {
        [Inject]
        protected NavigationManager Navigation { get; set; } = null!;

        protected override Task OnErrorAsync(Exception exception)
        {
            // Future:
            // - Serilog
            // - Telemetry
            // - Sentry
            // - Application Insights

            return Task.CompletedTask;
        }

        protected void Reload()
        {
            Recover();

            Navigation.NavigateTo(
                Navigation.Uri,
                forceLoad: true);
        }
    }
}
