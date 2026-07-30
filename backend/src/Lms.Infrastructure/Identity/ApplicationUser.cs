using Microsoft.AspNetCore.Identity;

namespace Lms.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
