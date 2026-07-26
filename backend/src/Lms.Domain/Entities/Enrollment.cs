using Lms.Domain.Common;
using Lms.Domain.Enums;

namespace Lms.Domain.Entities;

public class Enrollment : AuditableEntity
{
    /// <summary>Identity user id of the enrolled student.</summary>
    public Guid StudentId { get; set; }

    public Guid TrainingId { get; set; }
    public Training? Training { get; set; }

    public DateTime EnrolledAt { get; set; }

    /// <summary>Overall completion percentage (0-100).</summary>
    public int ProgressPercent { get; set; }

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    public DateTime? CompletedAt { get; set; }

    public ICollection<ModuleCompletion> ModuleCompletions { get; set; } = new List<ModuleCompletion>();

    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}

public class ModuleCompletion : AuditableEntity
{
    public Guid EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }

    public Guid ModuleId { get; set; }
    public Module? Module { get; set; }

    public DateTime CompletedAt { get; set; }
}

public class QuizAttempt : AuditableEntity
{
    public Guid EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }

    /// <summary>The quiz or exam activity that was attempted.</summary>
    public Guid ActivityId { get; set; }

    /// <summary>Achieved score as a percentage (0-100).</summary>
    public int Score { get; set; }

    public bool Passed { get; set; }

    public DateTime SubmittedAt { get; set; }
}
