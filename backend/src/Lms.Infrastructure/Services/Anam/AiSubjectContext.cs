namespace Lms.Infrastructure.Services.Anam;

/// <summary>
/// Read-only snapshot of the subject an AI trainer session is scoped to, shaped for prompt
/// building rather than for the API surface (hence Infrastructure rather than a DTO).
/// Loaded by <see cref="AnamAiTrainerService"/> and consumed by <see cref="AiTrainerPromptBuilder"/>.
/// </summary>
/// <param name="FocusModuleId">
/// Set when the caller scoped the session to a single module, so the persona teaches that
/// module while keeping the rest of the training as background.
/// </param>
public sealed record AiSubjectContext(
    Guid TrainingId,
    string Title,
    string? Description,
    string? CategoryName,
    string Difficulty,
    int DurationMinutes,
    string? TrainerName,
    IReadOnlyList<AiModuleContext> Modules,
    Guid? FocusModuleId)
{
    /// <summary>The module the session is focused on, when one was requested and still exists.</summary>
    public AiModuleContext? FocusModule =>
        FocusModuleId is null ? null : Modules.FirstOrDefault(m => m.Id == FocusModuleId);
}

/// <param name="Activities">
/// Pre-formatted "Kind: Title" lines (e.g. <c>Lesson: Overview</c>) in module order.
/// </param>
public sealed record AiModuleContext(
    Guid Id,
    string Title,
    string? Description,
    int Order,
    int Duration,
    IReadOnlyList<string> Activities);
