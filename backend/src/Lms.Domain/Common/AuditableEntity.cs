namespace Lms.Domain.Common;

/// <summary>
/// Base type for persisted aggregates. Carries a surrogate key and audit timestamps
/// that the DbContext maintains automatically on save.
/// </summary>
public abstract class AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
