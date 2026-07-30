namespace Lms.Application.Common;

public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Trainer = "Trainer";
    public const string Student = "Student";

    public static readonly string[] All = { Administrator, Trainer, Student };
}
