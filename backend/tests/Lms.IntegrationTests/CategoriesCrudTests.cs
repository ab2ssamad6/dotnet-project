using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Lms.Application.Common;
using Lms.Application.Dtos.Auth;
using Lms.Application.Dtos.Categories;

namespace Lms.IntegrationTests;

public class CategoriesCrudTests : IClassFixture<LmsWebApplicationFactory>
{
    private readonly LmsWebApplicationFactory _factory;

    public CategoriesCrudTests(LmsWebApplicationFactory factory) => _factory = factory;

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin@lms.local", "Admin#12345"));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        return client;
    }

    [Fact]
    public async Task Admin_Can_Create_Get_And_List_Categories()
    {
        var client = await CreateAdminClientAsync();
        var name = $"Category {Guid.NewGuid():N}";

        var create = await client.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest(name, "Integration test category"));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<CategoryDto>();
        created!.Name.Should().Be(name);

        var get = await client.GetAsync($"/api/categories/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await client.GetFromJsonAsync<PagedResult<CategoryDto>>("/api/categories?page=1&pageSize=100");
        list!.Items.Should().Contain(c => c.Id == created.Id);
    }

    [Fact]
    public async Task Creating_Duplicate_Category_Returns_409()
    {
        var client = await CreateAdminClientAsync();
        var name = $"Dup {Guid.NewGuid():N}";

        (await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest(name, null)))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest(name, null));
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Student_Cannot_Create_Category()
    {
        var client = _factory.CreateClient();
        var email = $"stu_{Guid.NewGuid():N}@lms.local";
        var reg = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("S", "T", email, "Str0ng#Pass1", "Str0ng#Pass1", "Student"));
        var auth = await reg.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var create = await client.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest($"X {Guid.NewGuid():N}", null));
        create.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
