using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RoomMate_Finder_Frontend;
using Microsoft.AspNetCore.Components.Authorization;
using RoomMate_Finder_Frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Set the HttpClient BaseAddress to the backend API URL.
// By default use the backend port from the server .env (http://localhost:5111).
// If you prefer you can provide an ApiBaseUrl in the Blazor app configuration.
var apiBase = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBase))
{
    apiBase = "http://localhost:5111";
}

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBase) });

// auth services
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ApiAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<ApiAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IConversationService, ConversationService>();

await builder.Build().RunAsync();