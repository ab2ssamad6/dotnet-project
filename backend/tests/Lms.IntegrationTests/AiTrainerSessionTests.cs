using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Lms.Application.Dtos.AiTrainer;
using Lms.Application.Dtos.Auth;
using Lms.Domain.Entities;
using Lms.Domain.Enums;
using Lms.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.IntegrationTests;

public class AiTrainerSessionTests : IClassFixture<LmsWebApplicationFactory>
{
    private readonly LmsWebApplicationFactory _factory;

    public AiTrainerSessionTests(LmsWebApplicationFactory factory) => _factory = factory;

    private async Task<HttpClient> CreateStudentClientAsync()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("student@lms.local", "Student#12345"));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        return client;
    }

    private async Task<(Guid TrainingId, Guid ModuleId)> SeedTrainingAsync(string title)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

        var lesson = new Lesson { Title = "Overview", Order = 1, Content = "The ML landscape." };
        var module = new Module
        {
            Title = "What is ML?",
            Description = "How machines learn from data.",
            Order = 1,
            Duration = 45,
            AiAvatarEnabled = true,
            Activities = new List<LearningActivity> { lesson },
        };
        var training = new Training
        {
            Title = title,
            Description = "Core concepts of supervised and unsupervised learning.",
            Difficulty = DifficultyLevel.Beginner,
            Duration = 360,
            Status = TrainingStatus.Published,
            Published = true,
            Category = new Category { Name = $"Data & AI {Guid.NewGuid():N}" },
            Trainer = new Trainer { FirstName = "Tina", LastName = "Trainer", Email = $"t_{Guid.NewGuid():N}@lms.local" },
            Modules = new List<Module> { module },
        };

        context.Trainings.Add(training);
        await context.SaveChangesAsync();

        return (training.Id, module.Id);
    }

    [Fact]
    public async Task Session_scoped_to_an_unknown_training_returns_404()
    {
        var client = await CreateStudentClientAsync();

        var response = await client.PostAsJsonAsync("/api/ai-trainer/session",
            new StartSessionRequest(null, null, Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Session_scoped_to_an_unknown_module_returns_404()
    {
        var client = await CreateStudentClientAsync();

        var response = await client.PostAsJsonAsync("/api/ai-trainer/session",
            new StartSessionRequest(Guid.NewGuid(), null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Session_scoped_to_a_real_training_resolves_the_subject()
    {
        var client = await CreateStudentClientAsync();
        var (trainingId, _) = await SeedTrainingAsync($"Machine Learning Foundations {Guid.NewGuid():N}");

        var response = await client.PostAsJsonAsync("/api/ai-trainer/session",
            new StartSessionRequest(null, null, trainingId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("not configured");
    }

    [Fact]
    public async Task Unscoped_session_skips_the_subject_lookup()
    {
        var client = await CreateStudentClientAsync();

        var response = await client.PostAsJsonAsync("/api/ai-trainer/session",
            new StartSessionRequest(null, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Module_presentation_names_the_parent_training_and_its_activities()
    {
        var client = await CreateStudentClientAsync();
        var title = $"Machine Learning Foundations {Guid.NewGuid():N}";
        var (_, moduleId) = await SeedTrainingAsync(title);

        var response = await client.PostAsJsonAsync("/api/ai-trainer/module-presentation",
            new ModulePresentationRequest(moduleId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ModulePresentationResponse>();
        payload!.Presentation.Should().Contain("What is ML?");
        payload.Presentation.Should().Contain(title);
        payload.Presentation.Should().Contain("Lesson: Overview");
    }
}
