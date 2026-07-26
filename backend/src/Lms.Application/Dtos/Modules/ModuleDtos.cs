namespace Lms.Application.Dtos.Modules;

public record ModuleDto(
    Guid Id,
    Guid TrainingId,
    string Title,
    string? Description,
    int Order,
    int Duration,
    string? VideoUrl,
    string? Attachment,
    bool AiAvatarEnabled,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateModuleRequest(
    Guid TrainingId,
    string Title,
    string? Description,
    int Order,
    int Duration,
    string? VideoUrl,
    string? Attachment,
    bool AiAvatarEnabled);

public record UpdateModuleRequest(
    string Title,
    string? Description,
    int Order,
    int Duration,
    string? VideoUrl,
    string? Attachment,
    bool AiAvatarEnabled);
