using Lms.Application.Common;
using Lms.Domain.Entities;
using Lms.Domain.Enums;
using Lms.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lms.Infrastructure.Persistence.Seed.DemoData;

/// <summary>
/// Populates the database with the demo catalogue described by <see cref="DemoCatalog"/>:
/// categories, trainers, extra accounts, four fully written courses and a spread of
/// enrollments with progress.
/// </summary>
/// <remarks>
/// Every step is idempotent and independently gated, so the seeder converges on a partially
/// populated database instead of failing or duplicating. Categories are matched by name and
/// trainers by e-mail because both carry unique indexes.
/// </remarks>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(
        LmsDbContext context,
        UserManager<ApplicationUser> userManager,
        ApplicationUser defaultTrainerUser,
        ApplicationUser defaultStudentUser,
        ILogger logger,
        CancellationToken ct = default)
    {
        var categories = await EnsureCategoriesAsync(context, ct);
        var trainers = await EnsureTrainersAsync(context, userManager, defaultTrainerUser, ct);

        foreach (var course in DemoCatalog.Courses)
        {
            var category = categories[course.CategoryName];
            var trainer = trainers[course.TrainerEmail];
            await EnsureCourseAsync(context, course.Title, () => course.Create(category, trainer), logger, ct);
        }

        await EnsureCourseAsync(
            context,
            DemoCatalog.KubernetesDraftTitle,
            () => DemoCatalog.CreateKubernetesDraft(
                categories[DemoCatalog.CloudAndDevOps],
                trainers[DemoCatalog.DefaultTrainerEmail]),
            logger,
            ct);

        await EnsureEnrollmentsAsync(context, userManager, defaultStudentUser, logger, ct);
    }

    private static async Task<Dictionary<string, Category>> EnsureCategoriesAsync(
        LmsDbContext context,
        CancellationToken ct)
    {
        var result = new Dictionary<string, Category>();

        foreach (var (name, description) in DemoCatalog.Categories)
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == name, ct);
            if (category is null)
            {
                category = new Category { Name = name, Description = description };
                context.Categories.Add(category);
            }

            result[name] = category;
        }

        await context.SaveChangesAsync(ct);
        return result;
    }

    private static async Task<Dictionary<string, Trainer>> EnsureTrainersAsync(
        LmsDbContext context,
        UserManager<ApplicationUser> userManager,
        ApplicationUser defaultTrainerUser,
        CancellationToken ct)
    {
        var result = new Dictionary<string, Trainer>();

        result[DemoCatalog.DefaultTrainerEmail] = await EnsureTrainerAsync(
            context,
            defaultTrainerUser.Email!,
            defaultTrainerUser.FirstName,
            defaultTrainerUser.LastName,
            "Senior instructor with a decade of hands-on delivery experience across .NET and cloud.",
            ".NET, Cloud, Data",
            "+1-555-0100",
            defaultTrainerUser.Id,
            ct);

        foreach (var profile in DemoCatalog.Trainers)
        {
            var user = await DbInitializer.SeedUserAsync(
                userManager,
                profile.Person.Email,
                DemoCatalog.DemoPassword,
                profile.Person.FirstName,
                profile.Person.LastName,
                Roles.Trainer);

            result[profile.Person.Email] = await EnsureTrainerAsync(
                context,
                profile.Person.Email,
                profile.Person.FirstName,
                profile.Person.LastName,
                profile.Biography,
                profile.Expertise,
                profile.Phone,
                user.Id,
                ct);
        }

        await context.SaveChangesAsync(ct);
        return result;
    }

    private static async Task<Trainer> EnsureTrainerAsync(
        LmsDbContext context,
        string email,
        string firstName,
        string lastName,
        string biography,
        string expertise,
        string phone,
        Guid userId,
        CancellationToken ct)
    {
        var trainer = await context.Trainers.FirstOrDefaultAsync(t => t.Email == email, ct);
        if (trainer is not null)
            return trainer;

        trainer = new Trainer
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Biography = biography,
            Expertise = expertise,
            Phone = phone,
            UserId = userId
        };
        context.Trainers.Add(trainer);
        return trainer;
    }

    /// <summary>
    /// Inserts a course unless one with the same title already exists. A pre-existing course with
    /// fewer modules than the demo version is the thin sample data seeded by earlier builds; it is
    /// replaced so that existing development databases pick up the richer content.
    /// </summary>
    private static async Task EnsureCourseAsync(
        LmsDbContext context,
        string title,
        Func<Training> create,
        ILogger logger,
        CancellationToken ct)
    {
        var demo = create();

        var existing = await context.Trainings
            .Include(t => t.Modules)
            .FirstOrDefaultAsync(t => t.Title == title, ct);

        if (existing is not null)
        {
            if (existing.Modules.Count >= demo.Modules.Count)
            {
                logger.LogInformation("Demo course {Title} already present; leaving it untouched.", title);
                return;
            }

            logger.LogWarning(
                "Replacing the earlier sample course {Title} ({OldModules} modules) with the demo version "
                    + "({NewModules} modules). Enrollments against the old course are removed with it.",
                title,
                existing.Modules.Count,
                demo.Modules.Count);

            context.Trainings.Remove(existing);
            await context.SaveChangesAsync(ct);
        }

        // A brand-new, untracked graph: EF marks every reachable entity as Added, so the whole
        // tree inserts in one go.
        context.Trainings.Add(demo);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Seeded demo course {Title} with {Modules} modules.", title, demo.Modules.Count);
    }

    private static async Task EnsureEnrollmentsAsync(
        LmsDbContext context,
        UserManager<ApplicationUser> userManager,
        ApplicationUser defaultStudentUser,
        ILogger logger,
        CancellationToken ct)
    {
        var students = new Dictionary<string, Guid>
        {
            [defaultStudentUser.Email!] = defaultStudentUser.Id
        };

        foreach (var person in DemoCatalog.Students)
        {
            var user = await DbInitializer.SeedUserAsync(
                userManager,
                person.Email,
                DemoCatalog.DemoPassword,
                person.FirstName,
                person.LastName,
                Roles.Student);
            students[person.Email] = user.Id;
        }

        var now = DateTime.UtcNow;
        var seeded = 0;

        foreach (var plan in DemoCatalog.EnrollmentsFor(defaultStudentUser.Email!))
        {
            if (!students.TryGetValue(plan.StudentEmail, out var studentId))
                continue;

            var training = await context.Trainings
                .Include(t => t.Modules)
                    .ThenInclude(m => m.Activities)
                .FirstOrDefaultAsync(t => t.Title == plan.CourseTitle, ct);

            if (training is null)
                continue;

            var alreadyEnrolled = await context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.TrainingId == training.Id, ct);
            if (alreadyEnrolled)
                continue;

            AddEnrollment(context, training, studentId, plan, now);
            seeded++;
        }

        if (seeded > 0)
        {
            await context.SaveChangesAsync(ct);
            logger.LogInformation("Seeded {Count} demo enrollments.", seeded);
        }
    }

    /// <summary>
    /// Builds one enrollment plus its completions and quiz attempts. Child rows are added through
    /// their own <see cref="DbSet{TEntity}"/> with scalar foreign keys rather than through the
    /// navigation of an already-tracked parent, which would make EF emit an UPDATE instead of an
    /// INSERT because <c>AuditableEntity</c> pre-sets the key.
    /// </summary>
    private static void AddEnrollment(
        LmsDbContext context,
        Training training,
        Guid studentId,
        DemoCatalog.DemoEnrollment plan,
        DateTime now)
    {
        var modules = training.Modules.OrderBy(m => m.Order).ToList();
        var completedCount = Math.Clamp(plan.CompletedModules, 0, modules.Count);
        var enrolledAt = now.AddDays(-(completedCount * 4 + 10));
        var isComplete = modules.Count > 0 && completedCount >= modules.Count;

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            TrainingId = training.Id,
            EnrolledAt = enrolledAt,
            // Mirrors EnrollmentService.MarkModuleCompleteAsync so the value does not jump the
            // first time the student completes a module through the API.
            ProgressPercent = modules.Count == 0
                ? 0
                : (int)Math.Round(completedCount * 100.0 / modules.Count),
            Status = isComplete ? EnrollmentStatus.Completed : EnrollmentStatus.Active,
            CompletedAt = isComplete ? enrolledAt.AddDays(completedCount) : null
        };

        context.Enrollments.Add(enrollment);

        // Deterministic per-student variation, so scores differ between students but stay stable
        // across re-seeds. Some land below the pass mark, giving the UI a failed attempt.
        var seed = plan.StudentEmail.Sum(c => (int)c);

        for (var i = 0; i < completedCount; i++)
        {
            var completedAt = enrolledAt.AddDays(i + 1);

            context.ModuleCompletions.Add(new ModuleCompletion
            {
                EnrollmentId = enrollment.Id,
                ModuleId = modules[i].Id,
                CompletedAt = completedAt
            });

            var assessment = modules[i].Activities
                .OfType<Assessment>()
                .OrderBy(a => a.Order)
                .FirstOrDefault();

            if (assessment is null)
                continue;

            var score = 56 + (seed + i * 17) % 42;
            context.QuizAttempts.Add(new QuizAttempt
            {
                EnrollmentId = enrollment.Id,
                ActivityId = assessment.Id,
                Score = score,
                Passed = score >= assessment.PassingScore,
                SubmittedAt = completedAt
            });
        }
    }
}
