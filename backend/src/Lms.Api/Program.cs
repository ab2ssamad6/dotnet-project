using Lms.Api.Endpoints;
using Lms.Api.Extensions;
using Lms.Api.Middleware;
using Lms.Application;
using Lms.Infrastructure;
using Lms.Infrastructure.Persistence;
using Lms.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);

    var app = builder.Build();

    // Apply migrations and seed (skipped under the integration-test host, which seeds itself).
    if (!app.Environment.IsEnvironment("Testing"))
    {
        await app.InitializeDatabaseAsync();
    }

    // Hosted behind a platform load balancer (Railway, Fly.io, ...) the real client address and
    // scheme only exist in the X-Forwarded-* headers; without this the per-IP rate limiter would
    // partition every visitor into the proxy's single address.
    if (app.Configuration.GetValue("Security:UseForwardedHeaders", false))
    {
        var forwardedHeaders = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto
                | ForwardedHeaders.XForwardedHost
        };
        // The platform's proxy address is dynamic, so no fixed allow-list can be configured.
        forwardedHeaders.KnownNetworks.Clear();
        forwardedHeaders.KnownProxies.Clear();
        app.UseForwardedHeaders(forwardedHeaders);
    }

    app.UseGlobalExceptionHandling();
    app.UseSerilogRequestLogging();
    app.UseSecurityHeaders();

    if (app.Configuration.GetValue("Security:UseHttpsRedirection", false))
    {
        app.UseHttpsRedirection();
    }

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(ApiServiceCollectionExtensions.CorsPolicy);
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).WithTags("System").AllowAnonymous();
    app.MapApplicationEndpoints();

    app.Run();
}
// Ignore the HostAbortedException raised by EF Core design-time tooling (e.g. 'dotnet ef migrations').
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "The LMS API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>Exposed so the integration-test WebApplicationFactory can reference the entry point.</summary>
public partial class Program;

internal static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
        await context.Database.MigrateAsync();
        await DbInitializer.SeedAsync(app.Services);
    }
}
