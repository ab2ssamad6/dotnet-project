using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Lms.Infrastructure.Identity;
using Lms.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Lms.Unittests;

public class TokenServiceTests
{
    private static TokenService CreateService() => new(Options.Create(new JwtOptions
    {
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        SigningKey = "unit-test-signing-key-that-is-long-enough-0123456789",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7
    }));

    [Fact]
    public void CreateAccessToken_embeds_user_claims_and_roles()
    {
        var service = CreateService();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "jane@lms.local",
            FirstName = "Jane",
            LastName = "Doe"
        };

        var token = service.CreateAccessToken(user, new[] { "Trainer" });
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.Token);
        jwt.Issuer.Should().Be("TestIssuer");
        jwt.Audiences.Should().Contain("TestAudience");
        jwt.Claims.Should().Contain(c => c.Value == "jane@lms.local");
        jwt.Claims.Should().Contain(c => c.Value == "Trainer");
        jwt.Claims.Should().Contain(c => c.Value == user.Id.ToString());
    }

    [Fact]
    public void CreateRefreshToken_produces_unique_active_tokens()
    {
        var service = CreateService();

        var first = service.CreateRefreshToken();
        var second = service.CreateRefreshToken();

        first.Token.Should().NotBeNullOrEmpty();
        first.Token.Should().NotBe(second.Token);
        first.IsActive.Should().BeTrue();
        first.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }
}
