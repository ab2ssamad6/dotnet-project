using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Lms.Application.Dtos.Auth;

namespace Lms.IntegrationTests;

public class AuthFlowTests : IClassFixture<LmsWebApplicationFactory>
{
    private readonly LmsWebApplicationFactory _factory;

    public AuthFlowTests(LmsWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_Login_Refresh_And_Access_Protected_Endpoint()
    {
        var client = _factory.CreateClient();
        var email = $"student_{Guid.NewGuid():N}@lms.local";

        // Register
        var register = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Sam", "Student", email, "Str0ng#Pass1", "Str0ng#Pass1", "Student"));
        register.StatusCode.Should().Be(HttpStatusCode.OK);
        var registered = await register.Content.ReadFromJsonAsync<AuthResponse>();
        registered!.User.Roles.Should().Contain("Student");

        // Login
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Str0ng#Pass1"));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.AccessToken.Should().NotBeNullOrEmpty();

        // Protected endpoint without token -> 401
        var unauthorized = await client.GetAsync("/api/categories");
        unauthorized.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Protected endpoint with token -> 200
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var authorized = await client.GetAsync("/api/categories");
        authorized.StatusCode.Should().Be(HttpStatusCode.OK);

        // Refresh rotates the token
        var refresh = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(auth.RefreshToken));
        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await refresh.Content.ReadFromJsonAsync<AuthResponse>();
        refreshed!.RefreshToken.Should().NotBe(auth.RefreshToken);
    }

    [Fact]
    public async Task Login_With_Wrong_Password_Returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin@lms.local", "wrong-password"));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_With_Weak_Password_Returns_400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("A", "B", $"x_{Guid.NewGuid():N}@lms.local", "weak", "weak", "Student"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
