using Lms.Domain.Common;

namespace Lms.Domain.Entities;

public class Category : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<Training> Trainings { get; set; } = new List<Training>();
}
