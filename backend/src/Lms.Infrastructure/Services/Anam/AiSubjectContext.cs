namespace Lms.Infrastructure.Services.Anam;

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
    public AiModuleContext? FocusModule =>
        FocusModuleId is null ? null : Modules.FirstOrDefault(m => m.Id == FocusModuleId);
}

public sealed record AiModuleContext(
    Guid Id,
    string Title,
    string? Description,
    int Order,
    int Duration,
    IReadOnlyList<string> Activities);
