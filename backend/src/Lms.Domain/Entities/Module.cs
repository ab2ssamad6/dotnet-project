using Lms.Domain.Common;

namespace Lms.Domain.Entities;

public class Module : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Order { get; set; }

    public int Duration { get; set; }

    public string? VideoUrl { get; set; }

    public string? Attachment { get; set; }

    public bool AiAvatarEnabled { get; set; }

    public Guid TrainingId { get; set; }
    public Training? Training { get; set; }

    public ICollection<LearningActivity> Activities { get; set; } = new List<LearningActivity>();
}
