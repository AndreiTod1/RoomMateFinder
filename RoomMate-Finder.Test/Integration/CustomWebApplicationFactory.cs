using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoomMate_Finder.Common;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Test.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// Replaces the real database with an in-memory database and sets up required services.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set required environment variables for the application
        Environment.SetEnvironmentVariable("API_PORT", "5000");
        Environment.SetEnvironmentVariable("JWT_KEY", "TestJwtKeyForIntegrationTesting123456789012345678901234567890");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");
        Environment.SetEnvironmentVariable("CORS_ORIGINS", "http://localhost:5000");
        Environment.SetEnvironmentVariable("DB_HOST", "localhost");
        Environment.SetEnvironmentVariable("DB_PORT", "5432");
        Environment.SetEnvironmentVariable("DB_NAME", "test");
        Environment.SetEnvironmentVariable("DB_USER", "test");
        Environment.SetEnvironmentVariable("DB_PASSWORD", "test");

        // Set Testing environment to skip migrations
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Find and remove the existing DbContext descriptor
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Replace JwtService with test configuration
            var jwtDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(JwtService));
            if (jwtDescriptor != null)
            {
                services.Remove(jwtDescriptor);
            }
            
            services.AddSingleton(new JwtService(
                "TestJwtKeyForIntegrationTesting123456789012345678901234567890",
                "TestIssuer",
                "TestAudience"
            ));

            // Ensure database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
