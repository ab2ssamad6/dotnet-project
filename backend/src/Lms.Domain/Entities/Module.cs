using Lms.Domain.Common;

namespace Lms.Domain.Entities;

public class Module : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Position of the module within its training (1-based).</summary>
    public int Order { get; set; }

    /// <summary>Estimated duration in minutes.</summary>
    public int Duration { get; set; }

    public string? VideoUrl { get; set; }

    public string? Attachment { get; set; }

    /// <summary>When enabled, the AI trainer avatar can present this module.</summary>
    public bool AiAvatarEnabled { get; set; }

    public Guid TrainingId { get; set; }
    public Training? Training { get; set; }

    public ICollection<LearningActivity> Activities { get; set; } = new List<LearningActivity>();
}
