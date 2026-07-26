namespace Lms.Application.Abstractions;

/// <summary>Ambient information about the authenticated caller for the current request.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);
}
