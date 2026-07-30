using Lms.Domain.Entities;
using Lms.Domain.Enums;

namespace Lms.Infrastructure.Persistence.Seed.DemoData;

public static class DemoCatalog
{
    public const string DemoPassword = "Demo#12345";

    public const string DataAndAi = "Data & AI";
    public const string WebDevelopment = "Web Development";
    public const string ProductAndDesign = "Product & Design";
    public const string Cybersecurity = "Cybersecurity";
    public const string CloudAndDevOps = "Cloud & DevOps";

    public const string DefaultTrainerEmail = "trainer@lms.local";

    public const string KubernetesDraftTitle = "Kubernetes in Practice";

    public sealed record DemoPerson(string Email, string FirstName, string LastName);

    public sealed record DemoTrainerProfile(
        DemoPerson Person,
        string Biography,
        string Expertise,
        string Phone);

    public sealed record DemoCourse(
        string Title,
        string CategoryName,
        string TrainerEmail,
        Func<Category, Trainer, Training> Create);

    public sealed record DemoEnrollment(string StudentEmail, string CourseTitle, int CompletedModules);

    public static readonly IReadOnlyList<(string Name, string Description)> Categories = new[]
    {
        (DataAndAi, "Applied AI, language models and the data work behind them."),
        (WebDevelopment, "Frontend and backend web technologies."),
        (ProductAndDesign, "Discovery, research and the craft of deciding what to build."),
        (Cybersecurity, "Application, network and operational security."),
        (CloudAndDevOps, "Cloud platforms, CI/CD and operations.")
    };

    public static readonly IReadOnlyList<DemoTrainerProfile> Trainers = new[]
    {
        new DemoTrainerProfile(
            new DemoPerson("sofia.reyes@lms.local", "Sofia", "Reyes"),
            "Machine learning engineer who now spends her time shipping language-model features and "
                + "explaining, patiently and repeatedly, why an evaluation set matters more than a clever prompt.",
            "LLM Applications, Retrieval, Evaluation",
            "+1-555-0142"),
        new DemoTrainerProfile(
            new DemoPerson("marcus.hale@lms.local", "Marcus", "Hale"),
            "Product designer and researcher. Fifteen years of watching people use software has left him "
                + "convinced that most roadmap arguments are really unresolved questions about the problem.",
            "Product Discovery, UX Research, Prototyping",
            "+1-555-0163"),
        new DemoTrainerProfile(
            new DemoPerson("priya.raman@lms.local", "Priya", "Raman"),
            "Application security engineer and incident responder. Has read enough breach reports to teach "
                + "security as a set of habits developers can keep rather than a checklist they resent.",
            "AppSec, Threat Modeling, Secure SDLC",
            "+1-555-0178")
    };

    public static readonly IReadOnlyList<DemoPerson> Students = new[]
    {
        new DemoPerson("jonas.weber@lms.local", "Jonas", "Weber"),
        new DemoPerson("mei.tanaka@lms.local", "Mei", "Tanaka"),
        new DemoPerson("rafael.costa@lms.local", "Rafael", "Costa")
    };

    public static readonly IReadOnlyList<DemoCourse> Courses = new[]
    {
        new DemoCourse(
            PromptEngineeringCourse.Title,
            DataAndAi,
            "sofia.reyes@lms.local",
            PromptEngineeringCourse.Create),
        new DemoCourse(
            ModernReactCourse.Title,
            WebDevelopment,
            DefaultTrainerEmail,
            ModernReactCourse.Create),
        new DemoCourse(
            ProductDiscoveryCourse.Title,
            ProductAndDesign,
            "marcus.hale@lms.local",
            ProductDiscoveryCourse.Create),
        new DemoCourse(
            ApplicationSecurityCourse.Title,
            Cybersecurity,
            "priya.raman@lms.local",
            ApplicationSecurityCourse.Create)
    };

    public static IReadOnlyList<DemoEnrollment> EnrollmentsFor(string seededStudentEmail) => new[]
    {
        new DemoEnrollment(seededStudentEmail, PromptEngineeringCourse.Title, 3),
        new DemoEnrollment(seededStudentEmail, ModernReactCourse.Title, 1),
        new DemoEnrollment(seededStudentEmail, ApplicationSecurityCourse.Title, 0),
        new DemoEnrollment("jonas.weber@lms.local", PromptEngineeringCourse.Title, 6),
        new DemoEnrollment("jonas.weber@lms.local", ProductDiscoveryCourse.Title, 2),
        new DemoEnrollment("mei.tanaka@lms.local", ModernReactCourse.Title, 6),
        new DemoEnrollment("mei.tanaka@lms.local", ApplicationSecurityCourse.Title, 2),
        new DemoEnrollment("rafael.costa@lms.local", ProductDiscoveryCourse.Title, 1)
    };

    public static Training CreateKubernetesDraft(Category category, Trainer trainer) => new()
    {
        Title = KubernetesDraftTitle,
        Description = "Operating containerised workloads at scale: scheduling, services, storage and "
            + "rollouts. This course is still being written and is not yet published.",
        Difficulty = DifficultyLevel.Advanced,
        Duration = 600,
        Status = TrainingStatus.Draft,
        Published = false,
        Category = category,
        Trainer = trainer
    };
}
