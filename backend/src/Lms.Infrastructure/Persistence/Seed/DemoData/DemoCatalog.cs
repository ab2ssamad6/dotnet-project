using Lms.Domain.Entities;
using Lms.Domain.Enums;

namespace Lms.Infrastructure.Persistence.Seed.DemoData;

/// <summary>
/// Declarative description of the demo catalogue: which categories, trainers, students,
/// courses and enrollments the seeder should ensure exist.
/// </summary>
public static class DemoCatalog
{
    /// <summary>Password used for every generated demo account. Meets the Identity policy.</summary>
    public const string DemoPassword = "Demo#12345";

    public const string DataAndAi = "Data & AI";
    public const string WebDevelopment = "Web Development";
    public const string BlockchainAndWeb3 = "Blockchain & Web3";
    public const string Cybersecurity = "Cybersecurity";
    public const string CloudAndDevOps = "Cloud & DevOps";

    /// <summary>The trainer seeded from <c>SeedOptions.TrainerEmail</c>; reused rather than recreated.</summary>
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

    /// <summary>One student's enrollment, expressed as "the first N modules are done".</summary>
    public sealed record DemoEnrollment(string StudentEmail, string CourseTitle, int CompletedModules);

    public static readonly IReadOnlyList<(string Name, string Description)> Categories = new[]
    {
        (DataAndAi, "Data engineering, machine learning and applied AI."),
        (WebDevelopment, "Frontend and backend web technologies."),
        (BlockchainAndWeb3, "Distributed ledgers, smart contracts and decentralised applications."),
        (Cybersecurity, "Application, network and operational security."),
        (CloudAndDevOps, "Cloud platforms, CI/CD and operations.")
    };

    /// <summary>
    /// Trainer profiles other than the default one. Each also gets an Identity account so the
    /// demo can be explored from the trainer's side.
    /// </summary>
    public static readonly IReadOnlyList<DemoTrainerProfile> Trainers = new[]
    {
        new DemoTrainerProfile(
            new DemoPerson("amina.farouk@lms.local", "Amina", "Farouk"),
            "Data scientist turned educator. Spent eight years building forecasting and recommendation "
                + "systems before deciding she preferred explaining them to building them.",
            "Machine Learning, Statistics, Python",
            "+1-555-0142"),
        new DemoTrainerProfile(
            new DemoPerson("diego.marin@lms.local", "Diego", "Marin"),
            "Distributed systems engineer who has audited smart contracts since 2017 and remains "
                + "cheerfully sceptical about most of what gets built on top of them.",
            "Blockchain, Solidity, Cryptography",
            "+1-555-0163"),
        new DemoTrainerProfile(
            new DemoPerson("nadia.kaur@lms.local", "Nadia", "Kaur"),
            "Application security lead and incident responder. Has run enough breach investigations to "
                + "believe firmly in boring controls applied consistently.",
            "AppSec, Threat Modeling, Incident Response",
            "+1-555-0178")
    };

    /// <summary>Extra student accounts, so lists and dashboards are not a single row.</summary>
    public static readonly IReadOnlyList<DemoPerson> Students = new[]
    {
        new DemoPerson("alice.dupont@lms.local", "Alice", "Dupont"),
        new DemoPerson("omar.benali@lms.local", "Omar", "Benali"),
        new DemoPerson("lena.novak@lms.local", "Lena", "Novak")
    };

    public static readonly IReadOnlyList<DemoCourse> Courses = new[]
    {
        new DemoCourse(
            MachineLearningFoundationsCourse.Title,
            DataAndAi,
            "amina.farouk@lms.local",
            MachineLearningFoundationsCourse.Create),
        new DemoCourse(
            AspNetCoreApisCourse.Title,
            WebDevelopment,
            DefaultTrainerEmail,
            AspNetCoreApisCourse.Create),
        new DemoCourse(
            BlockchainFundamentalsCourse.Title,
            BlockchainAndWeb3,
            "diego.marin@lms.local",
            BlockchainFundamentalsCourse.Create),
        new DemoCourse(
            CyberSecurityEssentialsCourse.Title,
            Cybersecurity,
            "nadia.kaur@lms.local",
            CyberSecurityEssentialsCourse.Create)
    };

    /// <summary>
    /// Enrollments for the seeded student plus the extra demo students, chosen to give the
    /// dashboards a spread of progress values and one completed course each for two students.
    /// The student e-mail is resolved against <c>SeedOptions.StudentEmail</c> for the first entries.
    /// </summary>
    public static IReadOnlyList<DemoEnrollment> EnrollmentsFor(string seededStudentEmail) => new[]
    {
        new DemoEnrollment(seededStudentEmail, MachineLearningFoundationsCourse.Title, 3),
        new DemoEnrollment(seededStudentEmail, AspNetCoreApisCourse.Title, 1),
        new DemoEnrollment(seededStudentEmail, CyberSecurityEssentialsCourse.Title, 0),
        new DemoEnrollment("alice.dupont@lms.local", MachineLearningFoundationsCourse.Title, 6),
        new DemoEnrollment("alice.dupont@lms.local", BlockchainFundamentalsCourse.Title, 2),
        new DemoEnrollment("omar.benali@lms.local", AspNetCoreApisCourse.Title, 6),
        new DemoEnrollment("omar.benali@lms.local", CyberSecurityEssentialsCourse.Title, 2),
        new DemoEnrollment("lena.novak@lms.local", BlockchainFundamentalsCourse.Title, 1)
    };

    /// <summary>
    /// A deliberately unpublished course, so admin screens have a Draft example that the
    /// student-facing catalogue must not show.
    /// </summary>
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
