using JobWize.Frontend;
using JobWize.Frontend.Modules.Identity;
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

builder.Services.AddScoped(
    _ => new HttpClient
    {
        BaseAddress = new Uri(apiBaseAddress)
    });

builder.Services.AddIdentityModule();

builder.Services.AddMudServices();

await builder.Build().RunAsync();