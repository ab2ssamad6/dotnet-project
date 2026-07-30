using Lms.Domain.Common;
using Lms.Domain.Enums;

namespace Lms.Domain.Entities;

public class Training : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Beginner;

    public int Duration { get; set; }

    public string? Thumbnail { get; set; }

    public TrainingStatus Status { get; set; } = TrainingStatus.Draft;

    public bool Published { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid TrainerId { get; set; }
    public Trainer? Trainer { get; set; }

    public ICollection<Module> Modules { get; set; } = new List<Module>();

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
