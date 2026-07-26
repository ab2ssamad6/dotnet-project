using Lms.Domain.Enums;

namespace Lms.Application.Dtos.Enrollments;

public record EnrollmentDto(
    Guid Id,
    Guid StudentId,
    Guid TrainingId,
    string? TrainingTitle,
    DateTime EnrolledAt,
    int ProgressPercent,
    EnrollmentStatus Status,
    DateTime? CompletedAt);

public record EnrollRequest(Guid TrainingId);

public record CompleteModuleRequest(Guid ModuleId);

public record SubmitQuizRequest(Guid ActivityId, IReadOnlyList<SubmittedAnswer> Answers);

public record SubmittedAnswer(Guid QuestionId, IReadOnlyList<Guid> SelectedAnswerIds);

public record QuizResultDto(
    Guid ActivityId,
    int Score,
    bool Passed,
    int CorrectCount,
    int TotalQuestions,
    DateTime SubmittedAt);

public record ModuleProgressDto(Guid ModuleId, string Title, int Order, bool Completed, DateTime? CompletedAt);

public record ProgressDto(
    Guid EnrollmentId,
    Guid TrainingId,
    string? TrainingTitle,
    int ProgressPercent,
    EnrollmentStatus Status,
    IReadOnlyList<ModuleProgressDto> Modules);

/// <summary>Placeholder for the future certificate feature (see roadmap).</summary>
public record CertificateDto(
    Guid EnrollmentId,
    Guid TrainingId,
    string? TrainingTitle,
    string StudentName,
    bool Available,
    DateTime? IssuedAt,
    string? Message);
