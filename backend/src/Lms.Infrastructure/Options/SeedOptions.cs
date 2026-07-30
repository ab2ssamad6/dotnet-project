namespace Lms.Infrastructure.Options;

public class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminEmail { get; set; } = "admin@lms.local";
    public string AdminPassword { get; set; } = "Admin#12345";
    public string TrainerEmail { get; set; } = "trainer@lms.local";
    public string TrainerPassword { get; set; } = "Trainer#12345";
    public string StudentEmail { get; set; } = "student@lms.local";
    public string StudentPassword { get; set; } = "Student#12345";
    public bool SeedSampleData { get; set; } = true;
}
