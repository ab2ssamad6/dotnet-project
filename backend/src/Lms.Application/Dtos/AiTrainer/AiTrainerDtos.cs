namespace Lms.Application.Dtos.AiTrainer;

public record StartSessionRequest(Guid? ModuleId, string? PersonaName, Guid? TrainingId = null);

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
