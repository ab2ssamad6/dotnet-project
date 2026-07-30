using Lms.Domain.Common;
using Lms.Domain.Enums;

namespace Lms.Domain.Entities;

public abstract class LearningActivity : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

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

public abstract class Assessment : LearningActivity
{
    public int PassingScore { get; set; } = 50;

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
