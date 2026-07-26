using Lms.Infrastructure.Persistence;
using Lms.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lms.IntegrationTests;

/// <summary>
/// Boots the API in the "Testing" environment against an in-memory SQLite database
/// (schema created via EnsureCreated, seeded with roles + default accounts).
/// </summary>
public class LmsWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MySql"] = "Server=localhost;Database=ignored;User=test;Password=test",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:SigningKey"] = "integration-test-signing-key-long-enough-0123456789-abc",
                ["AiTrainer:Anam:ApiKey"] = "",
                ["Seed:SeedSampleData"] = "false",
                ["Cors:AllowedOrigins:0"] = "http://localhost"
            });
        });

        builder.ConfigureServices(services =>
        {
            _connection.Open();

            // Replace the MySQL DbContext registration with in-memory SQLite.
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<LmsDbContext>)
                            || d.ServiceType == typeof(DbContextOptions)
                            || d.ServiceType == typeof(LmsDbContext))
                .ToList();
            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            services.AddDbContext<LmsDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
        await context.Database.EnsureCreatedAsync();
        await DbInitializer.SeedAsync(Services);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
