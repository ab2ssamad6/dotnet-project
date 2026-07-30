using System.Text;
using System.Threading.RateLimiting;
using Lms.Application.Common;
using Lms.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Lms.Api.Extensions;

public static class ApiServiceCollectionExtensions
{
    public const string CorsPolicy = "LmsCorsPolicy";
    public const string AuthRateLimitPolicy = "auth";

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddJwtAuthentication(configuration);
        services.AddAuthorizationPolicies();
        services.AddCorsPolicy(configuration);
        services.AddRateLimiting();
        services.AddSwagger();
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        return services;
    }

    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });
    }

    private static void AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Roles.Administrator, p => p.RequireRole(Roles.Administrator))
            .AddPolicy(Roles.Trainer, p => p.RequireRole(Roles.Trainer))
            .AddPolicy(Roles.Student, p => p.RequireRole(Roles.Student))
            .AddPolicy("ContentManager", p => p.RequireRole(Roles.Administrator, Roles.Trainer));
    }

    private static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = ReadAllowedOrigins(configuration);
        services.AddCors(options =>
            options.AddPolicy(CorsPolicy, policy =>
            {
                if (origins.Length > 0)
                    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                else
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            }));
    }

    private static string[] ReadAllowedOrigins(IConfiguration configuration)
    {
        var section = configuration.GetSection("Cors:AllowedOrigins");
        var scalar = section.Value;

        var origins = !string.IsNullOrWhiteSpace(scalar)
            ? scalar.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : section.Get<string[]>() ?? Array.Empty<string>();

        return origins.Select(origin => origin.TrimEnd('/')).ToArray();
    }

    private static void AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    PartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy(AuthRateLimitPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    PartitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });
    }

    private static string PartitionKey(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? Guid.NewGuid().ToString();

    private static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "LMS API",
                Version = "v1",
                Description = "Training Management System (Learning Management System) API."
            });

            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter the JWT access token (without the 'Bearer' prefix).",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };
            options.AddSecurityDefinition("Bearer", scheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
        });
    }
}
