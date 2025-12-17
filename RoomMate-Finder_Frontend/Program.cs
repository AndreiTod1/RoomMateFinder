using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RoomMate_Finder_Frontend;
using Microsoft.AspNetCore.Components.Authorization;
using RoomMate_Finder_Frontend.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Set the HttpClient BaseAddress to the backend API URL.
// By default use the backend port from the server .env (http://localhost:5111).
var apiBase = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBase))
{
    apiBase = "http://localhost:5111";
}

// Register AuthTokenHandler
builder.Services.AddScoped<AuthTokenHandler>();

// Configure HttpClient with the AuthTokenHandler to automatically add JWT token
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthTokenHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler) { BaseAddress = new Uri(apiBase) };
});

// MudBlazor services
builder.Services.AddMudServices();

// auth services
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ApiAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<ApiAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

await builder.Build().RunAsync();