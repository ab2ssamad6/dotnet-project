using Lms.Api.Extensions;
using Lms.Api.Filters;
using Lms.Application.Abstractions;
using Lms.Application.Abstractions.Services;
using Lms.Application.Dtos.Auth;

namespace Lms.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .RequireRateLimiting(ApiServiceCollectionExtensions.AuthRateLimitPolicy);

        group.MapPost("/register", async (RegisterRequest request, IAuthService service, CancellationToken ct) =>
                (await service.RegisterAsync(request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, RegisterRequest>()
            .AllowAnonymous()
            .WithSummary("Register a new Student or Trainer account.");

        group.MapPost("/login", async (LoginRequest request, IAuthService service, CancellationToken ct) =>
                (await service.LoginAsync(request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, LoginRequest>()
            .AllowAnonymous()
            .WithSummary("Authenticate and receive access + refresh tokens.");

        group.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService service, CancellationToken ct) =>
                (await service.RefreshTokenAsync(request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, RefreshTokenRequest>()
            .AllowAnonymous()
            .WithSummary("Exchange a refresh token for a new access token (rotates the refresh token).");

        group.MapPost("/logout", async (LogoutRequest request, IAuthService service, CancellationToken ct) =>
                (await service.LogoutAsync(request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, LogoutRequest>()
            .AllowAnonymous()
            .WithSummary("Revoke a refresh token.");

        group.MapPost("/forgot-password", async (ForgotPasswordRequest request, IAuthService service, CancellationToken ct) =>
                (await service.ForgotPasswordAsync(request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, ForgotPasswordRequest>()
            .AllowAnonymous()
            .WithSummary("Send a password-reset token to the account email.");

        group.MapPost("/reset-password", async (ResetPasswordRequest request, IAuthService service, CancellationToken ct) =>
                (await service.ResetPasswordAsync(request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, ResetPasswordRequest>()
            .AllowAnonymous()
            .WithSummary("Reset a password using a reset token.");

        group.MapPost("/verify-email", async (VerifyEmailRequest request, IAuthService service, CancellationToken ct) =>
                (await service.VerifyEmailAsync(request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, VerifyEmailRequest>()
            .AllowAnonymous()
            .WithSummary("Confirm an email address using a verification token.");

        group.MapPost("/change-password", async (
                ChangePasswordRequest request, IAuthService service, ICurrentUser currentUser, CancellationToken ct) =>
            {
                if (currentUser.UserId is not { } userId)
                    return Results.Unauthorized();
                return (await service.ChangePasswordAsync(userId, request, ct)).ToHttpResult();
            })
            .WithValidation<RouteHandlerBuilder, ChangePasswordRequest>()
            .RequireAuthorization()
            .WithSummary("Change the authenticated user's password.");

        return app;
    }
}
