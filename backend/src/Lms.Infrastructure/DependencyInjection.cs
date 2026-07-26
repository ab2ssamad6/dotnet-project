using System.Net.Http.Headers;
using Lms.Application.Abstractions;
using Lms.Application.Abstractions.Services;
using Lms.Infrastructure.Identity;
using Lms.Infrastructure.Options;
using Lms.Infrastructure.Persistence;
using Lms.Infrastructure.Services;
using Lms.Infrastructure.Services.Anam;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AnamOptions>(configuration.GetSection(AnamOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));

        // Database
        var connectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("Connection string 'MySql' is not configured.");
        var serverVersion = ResolveServerVersion(configuration, connectionString);
        services.AddDbContext<LmsDbContext>(options =>
            options.UseMySql(connectionString, serverVersion, my => my.EnableRetryOnFailure()));

        services.AddConfiguredIdentity();
        services.AddInfrastructureServices();

        return services;
    }

    /// <summary>
    /// Registers the DbContext against a caller-supplied provider (used by integration tests
    /// to swap MySQL for SQLite) plus all Identity and application services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> configureDb)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AnamOptions>(configuration.GetSection(AnamOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));

        services.AddDbContext<LmsDbContext>(configureDb);
        services.AddConfiguredIdentity();
        services.AddInfrastructureServices();
        return services;
    }

    private static IdentityBuilder AddConfiguredIdentity(this IServiceCollection services) =>
        services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireDigit = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.SignIn.RequireConfirmedEmail = false;
                })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<LmsDbContext>()
                .AddDefaultTokenProviders();

    private static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITrainerService, TrainerService>();
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IDashboardService, DashboardService>();

        services.AddHttpClient<IAITrainerService, AnamAiTrainerService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<AnamOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                client.BaseAddress = new Uri(options.BaseUrl);
            if (options.IsConfigured)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        });

        return services;
    }

    private static ServerVersion ResolveServerVersion(IConfiguration configuration, string connectionString)
    {
        // Prefer an explicit version to avoid a live connection during startup;
        // fall back to auto-detection only when asked.
        var configured = configuration["Database:ServerVersion"];
        if (!string.IsNullOrWhiteSpace(configured))
            return ServerVersion.Parse(configured);

        return new MySqlServerVersion(new Version(8, 0, 36));
    }
}
