using Lms.Domain.Common;
using Lms.Domain.Enums;

namespace Lms.Domain.Entities;

public class Question : AuditableEntity
{
    public string Text { get; set; } = string.Empty;

    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

    public int Points { get; set; } = 1;

    public Guid AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}

public class Answer : AuditableEntity
{
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public Guid QuestionId { get; set; }
    public Question? Question { get; set; }
}
