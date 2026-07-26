using Lms.Application.Abstractions;
using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Auth;
using Lms.Infrastructure.Identity;
using Lms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lms.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly LmsDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        LmsDbContext context,
        IEmailSender emailSender,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            return Result<AuthResponse>.Conflict("An account with this email already exists.");

        var role = request.Role ?? Roles.Student;
        // Only Student/Trainer may self-register; Administrator is provisioned via seeding.
        if (role == Roles.Administrator)
            return Result<AuthResponse>.Forbidden("Administrator accounts cannot be self-registered.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var created = await _userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
            return Result<AuthResponse>.Validation(Describe(created));

        await _userManager.AddToRoleAsync(user, role);

        // Send an email-verification link (logged in dev).
        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailSender.SendAsync(user.Email!, "Confirm your email",
            $"Confirm your account. UserId: {user.Id}, Token: {emailToken}", ct);

        _logger.LogInformation("User registered: {Email} as {Role}", user.Email, role);
        return await BuildAuthResponseAsync(user, ct);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogWarning("Failed login attempt for {Email}", request.Email);
            return Result<AuthResponse>.Unauthorized("Invalid credentials.");
        }

        if (await _userManager.IsLockedOutAsync(user))
            return Result<AuthResponse>.Forbidden("Account is locked. Try again later.");

        _logger.LogInformation("User logged in: {Email}", user.Email);
        return await BuildAuthResponseAsync(user, ct);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var stored = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, ct);

        if (stored is null || !stored.IsActive || stored.User is null)
            return Result<AuthResponse>.Unauthorized("Invalid or expired refresh token.");

        // Rotate: revoke the old token and issue a fresh one.
        var newRefresh = _tokenService.CreateRefreshToken();
        newRefresh.UserId = stored.UserId;
        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByToken = newRefresh.Token;
        _context.RefreshTokens.Add(newRefresh);
        await _context.SaveChangesAsync(ct);

        var roles = await _userManager.GetRolesAsync(stored.User);
        var access = _tokenService.CreateAccessToken(stored.User, roles);
        return Result<AuthResponse>.Success(
            new AuthResponse(access.Token, newRefresh.Token, access.ExpiresAt, ToUserDto(stored.User, roles)));
    }

    public async Task<Result> LogoutAsync(LogoutRequest request, CancellationToken ct = default)
    {
        var stored = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == request.RefreshToken, ct);
        if (stored is not null && stored.IsActive)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
        // Always succeed to avoid leaking token validity.
        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailSender.SendAsync(user.Email!, "Reset your password",
                $"Use this token to reset your password: {token}", ct);
            _logger.LogInformation("Password reset requested for {Email}", user.Email);
        }
        // Do not reveal whether the email exists.
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Validation("Invalid reset request.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return result.Succeeded ? Result.Success() : Result.Validation(Describe(result));
    }

    public async Task<Result> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result.Validation("Invalid verification request.");

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        return result.Succeeded ? Result.Success() : Result.Validation(Describe(result));
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.NotFound("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        return result.Succeeded ? Result.Success() : Result.Validation(Describe(result));
    }

    private async Task<Result<AuthResponse>> BuildAuthResponseAsync(ApplicationUser user, CancellationToken ct)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var access = _tokenService.CreateAccessToken(user, roles);

        var refresh = _tokenService.CreateRefreshToken();
        refresh.UserId = user.Id;
        _context.RefreshTokens.Add(refresh);
        await _context.SaveChangesAsync(ct);

        return Result<AuthResponse>.Success(
            new AuthResponse(access.Token, refresh.Token, access.ExpiresAt, ToUserDto(user, roles)));
    }

    private static UserDto ToUserDto(ApplicationUser user, IEnumerable<string> roles) =>
        new(user.Id, user.Email ?? string.Empty, user.FirstName, user.LastName, user.EmailConfirmed, roles.ToList());

    private static string Describe(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(e => e.Description));
}
