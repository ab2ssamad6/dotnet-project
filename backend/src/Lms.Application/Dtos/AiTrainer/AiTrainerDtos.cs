namespace Lms.Application.Dtos.AiTrainer;

/// <summary>
/// Request to open an AI trainer avatar session. Optionally scoped to a module so the
/// persona can be primed with that module's context.
/// </summary>
public record StartSessionRequest(Guid? ModuleId, string? PersonaName);

/// <summary>
/// Returned to the client. <see cref="SessionToken"/> is what the Anam.ai browser SDK
/// streams with (mirrors the original Node prototype's /api/session-token response).
/// </summary>
public record StartSessionResponse(
    string SessionToken,
    string Provider,
    Guid? ModuleId,
    DateTime IssuedAt);

public record AskQuestionRequest(string SessionToken, string Question, Guid? ModuleId);

public record AskQuestionResponse(string Answer, bool Live);

public record ModulePresentationRequest(Guid ModuleId);

public record ModulePresentationResponse(Guid ModuleId, string Presentation, bool Live);

public record StopSessionRequest(string SessionToken);
