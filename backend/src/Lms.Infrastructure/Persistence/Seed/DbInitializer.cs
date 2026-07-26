using Lms.Application.Common;
using Lms.Domain.Entities;
using Lms.Domain.Enums;
using Lms.Infrastructure.Identity;
using Lms.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lms.Infrastructure.Persistence.Seed;

/// <summary>Applies migrations and seeds roles, default accounts and sample data. Idempotent.</summary>
public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var context = sp.GetRequiredService<LmsDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var options = sp.GetRequiredService<IOptions<SeedOptions>>().Value;
        var logger = sp.GetRequiredService<ILogger<LmsDbContext>>();

        await SeedRolesAsync(roleManager);
        var admin = await SeedUserAsync(userManager, options.AdminEmail, options.AdminPassword, "System", "Administrator", Roles.Administrator);
        var trainerUser = await SeedUserAsync(userManager, options.TrainerEmail, options.TrainerPassword, "Tina", "Trainer", Roles.Trainer);
        await SeedUserAsync(userManager, options.StudentEmail, options.StudentPassword, "Sam", "Student", Roles.Student);

        if (!options.SeedSampleData)
        {
            logger.LogInformation("Seeding complete (sample data disabled).");
            return;
        }

        if (await context.Trainings.AnyAsync(ct))
        {
            logger.LogInformation("Seeding complete (sample data already present).");
            return;
        }

        await SeedSampleDataAsync(context, trainerUser, ct);
        logger.LogInformation("Seeding complete (with sample data).");
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    private static async Task<ApplicationUser> SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string email, string password, string firstName, string lastName, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
            return user;

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, role);
        return user;
    }

    private static async Task SeedSampleDataAsync(LmsDbContext context, ApplicationUser trainerUser, CancellationToken ct)
    {
        var web = new Category { Name = "Web Development", Description = "Frontend and backend web technologies." };
        var data = new Category { Name = "Data & AI", Description = "Data engineering, machine learning and AI." };
        var cloud = new Category { Name = "Cloud & DevOps", Description = "Cloud platforms, CI/CD and operations." };
        context.Categories.AddRange(web, data, cloud);

        var trainer = new Trainer
        {
            FirstName = trainerUser.FirstName,
            LastName = trainerUser.LastName,
            Email = trainerUser.Email!,
            Biography = "Senior instructor with a decade of hands-on delivery experience.",
            Expertise = ".NET, Cloud, Data",
            Phone = "+1-555-0100",
            UserId = trainerUser.Id
        };
        context.Trainers.Add(trainer);

        var aspNet = new Training
        {
            Title = "Building APIs with ASP.NET Core",
            Description = "Design and ship production-ready REST APIs with ASP.NET Core and EF Core.",
            Difficulty = DifficultyLevel.Intermediate,
            Duration = 480,
            Status = TrainingStatus.Published,
            Published = true,
            Category = web,
            Trainer = trainer,
            Modules = new List<Module>
            {
                new()
                {
                    Title = "Getting Started",
                    Description = "Project setup and fundamentals.",
                    Order = 1,
                    Duration = 60,
                    AiAvatarEnabled = true,
                    Activities = new List<LearningActivity>
                    {
                        new Lesson { Title = "Introduction", Order = 1, Content = "Welcome to the course." },
                        new Quiz
                        {
                            Title = "Warm-up Quiz",
                            Order = 2,
                            PassingScore = 50,
                            Questions = new List<Question>
                            {
                                new()
                                {
                                    Text = "ASP.NET Core Minimal APIs require controllers.",
                                    Type = QuestionType.TrueFalse,
                                    Points = 1,
                                    Answers = new List<Answer>
                                    {
                                        new() { Text = "True", IsCorrect = false },
                                        new() { Text = "False", IsCorrect = true }
                                    }
                                },
                                new()
                                {
                                    Text = "Which verb is idempotent?",
                                    Type = QuestionType.MultipleChoice,
                                    Points = 1,
                                    Answers = new List<Answer>
                                    {
                                        new() { Text = "POST", IsCorrect = false },
                                        new() { Text = "PUT", IsCorrect = true },
                                        new() { Text = "PATCH", IsCorrect = false }
                                    }
                                }
                            }
                        }
                    }
                },
                new()
                {
                    Title = "Persistence with EF Core",
                    Description = "Modeling and querying data.",
                    Order = 2,
                    Duration = 90,
                    Activities = new List<LearningActivity>
                    {
                        new Lesson { Title = "DbContext basics", Order = 1, Content = "Configuring the context." }
                    }
                }
            }
        };

        var ml = new Training
        {
            Title = "Machine Learning Foundations",
            Description = "Core concepts of supervised and unsupervised learning.",
            Difficulty = DifficultyLevel.Beginner,
            Duration = 360,
            Status = TrainingStatus.Published,
            Published = true,
            Category = data,
            Trainer = trainer,
            Modules = new List<Module>
            {
                new()
                {
                    Title = "What is ML?",
                    Order = 1,
                    Duration = 45,
                    Activities = new List<LearningActivity>
                    {
                        new Lesson { Title = "Overview", Order = 1, Content = "The ML landscape." }
                    }
                }
            }
        };

        var draft = new Training
        {
            Title = "Kubernetes in Practice",
            Description = "Operating containerized workloads at scale.",
            Difficulty = DifficultyLevel.Advanced,
            Duration = 600,
            Status = TrainingStatus.Draft,
            Published = false,
            Category = cloud,
            Trainer = trainer
        };

        context.Trainings.AddRange(aspNet, ml, draft);
        await context.SaveChangesAsync(ct);
    }
}
