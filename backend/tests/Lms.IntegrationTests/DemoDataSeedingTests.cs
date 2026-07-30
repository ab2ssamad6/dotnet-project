using FluentAssertions;
using Lms.Domain.Entities;
using Lms.Domain.Enums;
using Lms.Infrastructure.Persistence;
using Lms.Infrastructure.Persistence.Seed;
using Lms.Infrastructure.Persistence.Seed.DemoData;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.IntegrationTests;

public class DemoDataWebApplicationFactory : LmsWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:SeedSampleData"] = "true"
            });
        });
    }
}

public class DemoDataSeedingTests : IClassFixture<DemoDataWebApplicationFactory>
{
    private readonly DemoDataWebApplicationFactory _factory;

    public DemoDataSeedingTests(DemoDataWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Seeds_the_four_demo_courses_with_full_content()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

        foreach (var expected in DemoCatalog.Courses)
        {
            var training = await context.Trainings
                .Include(t => t.Modules)
                    .ThenInclude(m => m.Activities)
                .SingleOrDefaultAsync(t => t.Title == expected.Title);

            training.Should().NotBeNull("{0} should have been seeded exactly once", expected.Title);
            training!.Published.Should().BeTrue();
            training.Status.Should().Be(TrainingStatus.Published);
            training.Duration.Should().BeGreaterThan(0);
            training.Modules.Should().HaveCount(6);
            training.Modules.Should().OnlyContain(m => m.Description != null && m.Description.Length > 0);

            var activities = training.Modules.SelectMany(m => m.Activities).ToList();
            activities.OfType<Lesson>().Should().HaveCount(10);
            activities.OfType<Exercise>().Should().HaveCount(5);
            activities.OfType<Quiz>().Should().HaveCount(5);
            activities.OfType<Exam>().Should().HaveCount(1);
        }

        (await context.Activities.OfType<Lesson>().CountAsync()).Should().Be(40);
        (await context.Activities.OfType<Exercise>().CountAsync()).Should().Be(20);
        (await context.Activities.OfType<Quiz>().CountAsync()).Should().Be(20);
        (await context.Activities.OfType<Exam>().CountAsync()).Should().Be(4);

        (await context.Questions.CountAsync()).Should().Be(132);
        (await context.Answers.CountAsync()).Should().Be(480);
    }

    [Fact]
    public async Task Seeds_the_unpublished_draft_course()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

        var draft = await context.Trainings
            .SingleOrDefaultAsync(t => t.Title == DemoCatalog.KubernetesDraftTitle);

        draft.Should().NotBeNull();
        draft!.Published.Should().BeFalse("admin screens need a Draft example the catalogue hides");
        draft.Status.Should().Be(TrainingStatus.Draft);
    }

    [Fact]
    public async Task Seeds_trainers_students_and_categories()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

        foreach (var (name, _) in DemoCatalog.Categories)
            (await context.Categories.CountAsync(c => c.Name == name)).Should().Be(1);

        foreach (var profile in DemoCatalog.Trainers)
        {
            var trainer = await context.Trainers.SingleOrDefaultAsync(t => t.Email == profile.Person.Email);
            trainer.Should().NotBeNull();
            trainer!.UserId.Should().NotBeNull().And.NotBe(Guid.Empty);
        }
    }

    [Fact]
    public async Task Progress_matches_the_formula_the_enrollment_service_uses()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

        var enrollments = await context.Enrollments
            .Include(e => e.ModuleCompletions)
            .ToListAsync();

        enrollments.Should().HaveCount(DemoCatalog.EnrollmentsFor("student@lms.local").Count);

        foreach (var enrollment in enrollments)
        {
            var totalModules = await context.Modules.CountAsync(m => m.TrainingId == enrollment.TrainingId);
            var completed = enrollment.ModuleCompletions.Count;

            var expected = totalModules == 0 ? 0 : (int)Math.Round(completed * 100.0 / totalModules);
            enrollment.ProgressPercent.Should().Be(expected);

            if (completed >= totalModules && totalModules > 0)
            {
                enrollment.Status.Should().Be(EnrollmentStatus.Completed);
                enrollment.CompletedAt.Should().NotBeNull();
            }
            else
            {
                enrollment.Status.Should().Be(EnrollmentStatus.Active);
                enrollment.CompletedAt.Should().BeNull();
            }
        }

        enrollments.Should().Contain(e => e.ProgressPercent == 100, "one student finished a course");
        enrollments.Should().Contain(e => e.ProgressPercent == 0, "one student just enrolled");
    }

    [Fact]
    public async Task Quiz_attempts_reference_real_assessments()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

        var attempts = await context.QuizAttempts.ToListAsync();
        attempts.Should().NotBeEmpty();

        foreach (var attempt in attempts)
        {
            var assessment = await context.Activities.OfType<Assessment>()
                .SingleOrDefaultAsync(a => a.Id == attempt.ActivityId);

            assessment.Should().NotBeNull("QuizAttempt.ActivityId has no FK, so it must be checked");
            attempt.Score.Should().BeInRange(0, 100);
            attempt.Passed.Should().Be(attempt.Score >= assessment!.PassingScore);
        }
    }

    [Fact]
    public async Task Re_running_the_seeder_changes_nothing()
    {
        var before = await SnapshotAsync();

        await DbInitializer.SeedAsync(_factory.Services);

        var after = await SnapshotAsync();
        after.Should().BeEquivalentTo(before, "the seeder is gated per course, per enrollment and per user");
    }

    private async Task<Dictionary<string, int>> SnapshotAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

        return new Dictionary<string, int>
        {
            ["categories"] = await context.Categories.CountAsync(),
            ["trainers"] = await context.Trainers.CountAsync(),
            ["trainings"] = await context.Trainings.CountAsync(),
            ["modules"] = await context.Modules.CountAsync(),
            ["activities"] = await context.Activities.CountAsync(),
            ["questions"] = await context.Questions.CountAsync(),
            ["answers"] = await context.Answers.CountAsync(),
            ["enrollments"] = await context.Enrollments.CountAsync(),
            ["completions"] = await context.ModuleCompletions.CountAsync(),
            ["attempts"] = await context.QuizAttempts.CountAsync()
        };
    }
}
