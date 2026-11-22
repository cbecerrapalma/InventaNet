using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Globalization;
using CBP.Blazor.Formatting;
using CBP.Blazor.Mud;
using Inventa.Store.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// S93.2: cultura es-CL determinista sin ICU (se complementa con
// applicationCulture en wwwroot/index.html).
CultureInfo.DefaultThreadCurrentCulture = FormatoCl.Cultura;
CultureInfo.DefaultThreadCurrentUICulture = FormatoCl.Cultura;
builder.RootComponents.Add<Inventa.Store.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
    throw new InvalidOperationException("ApiBaseUrl not configured. Add ApiBaseUrl to wwwroot/appsettings.json");
if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri))
    throw new InvalidOperationException($"ApiBaseUrl invalid: {apiBaseUrl}");

builder.Services.AddMudServices();
builder.Services.AddMudEsCl(); // S93.2: textos MudBlazor en espanol
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = apiUri });
builder.Services.AddScoped<CatalogApiService>();
builder.Services.AddScoped<StorefrontApiService>();
builder.Services.AddScoped<StorefrontState>();
builder.Services.AddScoped<StoreSessionStorage>();
builder.Services.AddScoped<IStoreAuthService, StoreAuthService>();
builder.Services.AddScoped<IContactApiService, ContactApiService>();
builder.Services.AddScoped<StoreAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<StoreAuthenticationStateProvider>());

await builder.Build().RunAsync();