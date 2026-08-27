using JobWize.Frontend;
using JobWize.Frontend.Modules.Dashboard;
using JobWize.Frontend.Modules.Identity;
using JobWize.Frontend.Modules.Applications;
using JobWize.Frontend.Shared.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string apiBaseAddress =
    builder.Configuration["Api:BaseAddress"]
    ?? throw new InvalidOperationException(
        "The API base address is not configured.");


builder.Services.AddTransient<AuthenticationHandler>();

builder.Services
    .AddHttpClient("Api", client =>
    {
        client.BaseAddress = new Uri(apiBaseAddress);
    })
    .AddHttpMessageHandler<AuthenticationHandler>();

builder.Services.AddHttpClient("AnonymousApi", client =>
{
    client.BaseAddress = new Uri(apiBaseAddress);
});


builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ITokenStorage, LocalStorageTokenStorage>();
builder.Services.AddScoped<JobWizeAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JobWizeAuthenticationStateProvider>());
builder.Services.AddScoped<TokenRefreshService>();

builder.Services.AddIdentityModule();
builder.Services.AddDashboardModule();
builder.Services.AddApplicationsModule();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
