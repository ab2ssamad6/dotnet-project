namespace Lms.Infrastructure.Identity;

public record AccessToken(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    AccessToken CreateAccessToken(ApplicationUser user, IEnumerable<string> roles);
    RefreshToken CreateRefreshToken();
}
