namespace Lms.Infrastructure.Identity;

/// <summary>A rotating refresh token bound to a single user.</summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }

    /// <summary>Set when this token is rotated, pointing at its replacement.</summary>
    public string? ReplacedByToken { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt is null && !IsExpired;
}
