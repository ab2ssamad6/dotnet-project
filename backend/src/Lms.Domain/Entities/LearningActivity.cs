using Lms.Domain.Common;
using Lms.Domain.Enums;

namespace Lms.Domain.Entities;

/// <summary>
/// Abstract base for everything a module can contain. Persisted with EF Core
/// table-per-hierarchy (TPH); <see cref="ActivityType"/> is the discriminator.
/// </summary>
public abstract class LearningActivity : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

    /// <summary>Position of the activity within its module (1-based).</summary>
    public int Order { get; set; }

    public abstract ActivityType ActivityType { get; }

    public Guid ModuleId { get; set; }
    public Module? Module { get; set; }
}

public class Lesson : LearningActivity
{
    public override ActivityType ActivityType => ActivityType.Lesson;

    public string? Content { get; set; }

    public string? VideoUrl { get; set; }
}

public class Exercise : LearningActivity
{
    public override ActivityType ActivityType => ActivityType.Exercise;

    public string? Instructions { get; set; }

    public string? ExpectedOutcome { get; set; }
}

/// <summary>
/// Shared base for question-based activities (<see cref="Quiz"/>, <see cref="Exam"/>).
/// Still part of the same TPH table.
/// </summary>
public abstract class Assessment : LearningActivity
{
    /// <summary>Minimum percentage (0-100) required to pass.</summary>
    public int PassingScore { get; set; } = 50;

    /// <summary>Time limit in minutes; null means untimed.</summary>
    public int? DurationMinutes { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}

public class Quiz : Assessment
{
    public override ActivityType ActivityType => ActivityType.Quiz;
}

public class Exam : Assessment
{
    public override ActivityType ActivityType => ActivityType.Exam;
}
