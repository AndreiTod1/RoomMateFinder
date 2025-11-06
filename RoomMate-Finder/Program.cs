using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using MediatR;
using DotNetEnv;
using RoomMate_Finder.Common;
using RoomMate_Finder.Features.Profiles;
using RoomMate_Finder.Infrastructure.Persistence;
using RoomMate_Finder.Validators;

LoadEnvironmentVariables();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // use only .env
    ApplicationName = typeof(Program).Assembly.FullName
});

// Configure port from .env 
ConfigurePort(builder);

// Configure services
ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();

await InitializeDatabaseAsync(app);

// Configure middleware
ConfigureMiddleware(app);

// Configure endpoints
ConfigureEndpoints(app);

app.Run();

// ===== helper =====

static void LoadEnvironmentVariables()
{
    try
    {
        Env.Load();
        
        var envFile = Path.Combine(AppContext.BaseDirectory, ".env");
        if (File.Exists(envFile))
        {
            Env.Load(envFile);
        }
        
        Console.WriteLine("✓ Environment variables loaded successfully from .env");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"✗ ERROR: Could not load .env file: {ex.Message}");
        Console.Error.WriteLine("Please ensure the .env file exists and is properly formatted.");
        throw;
    }
}

static void ConfigurePort(WebApplicationBuilder builder)
{
    var port = GetRequiredEnvironmentVariable("API_PORT");
    var url = $"http://localhost:{port}";
    
    builder.WebHost.UseUrls(url);

    Console.WriteLine($"✓ Server configured to run on {url}");
}

static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
{
    // JWT Configuration - from .env
    ConfigureJwtAuthentication(services);
    
    // CORS Configuration - from .env, with development fallbacks
    ConfigureCors(services, env);
    
    // Database Configuration - from.env
    ConfigureDatabase(services);
    
    // MediatR & Validation
    services.AddMediatR(cfg => 
        cfg.RegisterServicesFromAssemblyContaining<CreateProfileRequest>());
    services.AddValidatorsFromAssemblyContaining<CreateProfileValidator>();
    
    // API Documentation
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}

static void ConfigureJwtAuthentication(IServiceCollection services)
{
    var jwtKey = GetRequiredEnvironmentVariable("JWT_KEY");
    var jwtIssuer = GetRequiredEnvironmentVariable("JWT_ISSUER");
    var jwtAudience = GetRequiredEnvironmentVariable("JWT_AUDIENCE");
    
    services.AddSingleton(new JwtService(jwtKey, jwtIssuer, jwtAudience));
    
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });
    
    services.AddAuthorization();
    
    Console.WriteLine($"✓ JWT configured from .env (Issuer: {jwtIssuer})");
}

static void ConfigureCors(IServiceCollection services, IWebHostEnvironment? env = null)
{
    var corsOriginsStr = GetRequiredEnvironmentVariable("CORS_ORIGINS");
    var allowedOrigins = corsOriginsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    
    // If we're in development, include common Blazor/ASP.NET dev ports so the frontend can call the API
    if (env?.IsDevelopment() == true)
    {
        // Add typical development origins used by Blazor WebAssembly and dotnet run
        var devOrigins = new[] { "http://localhost:5042", "https://localhost:7100", "http://localhost:5173", "http://localhost:3000" };
        foreach (var o in devOrigins)
        {
            if (!allowedOrigins.Contains(o, StringComparer.OrdinalIgnoreCase))
            {
                allowedOrigins.Add(o);
            }
        }
    }

    // Also add common local dev origins as a safe default for local development
    // (this avoids issues if ASPNETCORE_ENVIRONMENT isn't set to Development for the backend process).
    var alwaysDevOrigins = new[] { "http://localhost:5042", "https://localhost:7100", "http://localhost:5173", "http://localhost:3000" };
    foreach (var o in alwaysDevOrigins)
    {
        if (!allowedOrigins.Contains(o, StringComparer.OrdinalIgnoreCase))
        {
            allowedOrigins.Add(o);
        }
    }
     
    services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(allowedOrigins.ToArray())
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });
    
    Console.WriteLine($"✓ CORS configured from .env: {string.Join(", ", allowedOrigins)}");
}

static void ConfigureDatabase(IServiceCollection services)
{
    var host = GetRequiredEnvironmentVariable("DB_HOST");
    var port = GetRequiredEnvironmentVariable("DB_PORT");
    var database = GetRequiredEnvironmentVariable("DB_NAME");
    var username = GetRequiredEnvironmentVariable("DB_USER");
    var password = GetRequiredEnvironmentVariable("DB_PASSWORD");
    
    var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    
    services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
    
    // Log connection string (fără parola)
    var maskedConnectionString = $"Host={host};Port={port};Database={database};Username={username};Password=*****";
    Console.WriteLine($"✓ Database configured from .env: {maskedConnectionString}");
}

static string GetRequiredEnvironmentVariable(string key)
{
    var value = Environment.GetEnvironmentVariable(key);
    
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException(
            $"✗ ERROR: Required environment variable '{key}' is not set in .env file.");
    }
    
    return value;
}

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    try
    {
        await dbContext.Database.EnsureCreatedAsync();
        Console.WriteLine("✓ Database initialized successfully");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"✗ Database initialization failed: {ex.Message}");
        Console.Error.WriteLine(ex.StackTrace);
        throw;
    }
}

static void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        Console.WriteLine("✓ Swagger UI available at /swagger");
    }
    
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
}

static void ConfigureEndpoints(WebApplication app)
{
    app.MapProfilesEndpoints();
    Console.WriteLine("✓ Endpoints configured");
}
