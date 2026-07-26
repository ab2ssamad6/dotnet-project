using Lms.Domain.Common;

namespace Lms.Domain.Entities;

public class Trainer : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Biography { get; set; }

    public string? Avatar { get; set; }

    public string? Expertise { get; set; }

    public string? Phone { get; set; }

    /// <summary>Optional link to the Identity user that owns this trainer profile.</summary>
    public Guid? UserId { get; set; }

    public ICollection<Training> Trainings { get; set; } = new List<Training>();
}
