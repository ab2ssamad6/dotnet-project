namespace Lms.Application.Common;

/// <summary>Canonical role names used across authorization policies and seeding.</summary>
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Trainer = "Trainer";
    public const string Student = "Student";

    public static readonly string[] All = { Administrator, Trainer, Student };
}
