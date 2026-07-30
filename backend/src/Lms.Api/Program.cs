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

    if (!app.Environment.IsEnvironment("Testing"))
    {
        await app.InitializeDatabaseAsync();
    }

    if (app.Configuration.GetValue("Security:UseForwardedHeaders", false))
    {
        var forwardedHeaders = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto
                | ForwardedHeaders.XForwardedHost
        };
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
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "The LMS API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

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
