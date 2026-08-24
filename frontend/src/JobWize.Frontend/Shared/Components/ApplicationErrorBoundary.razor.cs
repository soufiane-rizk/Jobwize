using Microsoft.AspNetCore.Components;

namespace JobWize.Frontend.Shared.Components
{
    public partial class ApplicationErrorBoundary : ErrorBoundaryBase
    {
        [Inject]
        protected ILogger<ApplicationErrorBoundary> Logger { get; set; } = null!;

        [Inject]
        protected NavigationManager Navigation { get; set; } = null!;

        protected override Task OnErrorAsync(Exception exception)
        {
            Logger.LogError(exception, "An unhandled exception occurred in the frontend.");

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
