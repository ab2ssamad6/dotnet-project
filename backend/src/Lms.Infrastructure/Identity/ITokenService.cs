namespace Lms.Infrastructure.Identity;

public record AccessToken(string Token, DateTime ExpiresAt);

/// <summary>Issues JWT access tokens and opaque refresh tokens.</summary>
public interface ITokenService
{
    AccessToken CreateAccessToken(ApplicationUser user, IEnumerable<string> roles);
    RefreshToken CreateRefreshToken();
}
