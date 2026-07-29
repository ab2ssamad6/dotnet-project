namespace Lms.Application.Dtos.AiTrainer;

/// <summary>
/// Request to open an AI trainer avatar session. Optionally scoped to a training and/or a
/// module so the persona is primed as a tutor for that subject. When <see cref="ModuleId"/>
/// is supplied its parent training is resolved automatically, and the avatar narrows its
/// teaching to that module.
/// </summary>
public record StartSessionRequest(Guid? ModuleId, string? PersonaName, Guid? TrainingId = null);

/// <summary>
/// Returned to the client. <see cref="SessionToken"/> is what the Anam.ai browser SDK
/// streams with (mirrors the original Node prototype's /api/session-token response).
/// <see cref="SubjectTitle"/> and <see cref="PersonaName"/> echo back the subject the persona
/// was actually primed with, so the UI can label the session.
/// </summary>
public record StartSessionResponse(
    string SessionToken,
    string Provider,
    Guid? ModuleId,
    DateTime IssuedAt,
    Guid? TrainingId = null,
    string? SubjectTitle = null,
    string? PersonaName = null);

public record AskQuestionRequest(string SessionToken, string Question, Guid? ModuleId);

public record AskQuestionResponse(string Answer, bool Live);

public record ModulePresentationRequest(Guid ModuleId);

public record ModulePresentationResponse(Guid ModuleId, string Presentation, bool Live);

public record StopSessionRequest(string SessionToken);
