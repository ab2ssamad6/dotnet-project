using Lms.Domain.Enums;

namespace Lms.Application.Dtos.Activities;

/// <summary>Flat, read-oriented projection covering every learning-activity type.</summary>
public record ActivityDto(
    Guid Id,
    Guid ModuleId,
    ActivityType Type,
    string Title,
    int Order,
    // Lesson
    string? Content,
    string? VideoUrl,
    // Exercise
    string? Instructions,
    string? ExpectedOutcome,
    // Quiz / Exam
    int? PassingScore,
    int? DurationMinutes,
    IReadOnlyList<QuestionDto>? Questions);

public record QuestionDto(
    Guid Id,
    string Text,
    QuestionType Type,
    int Points,
    IReadOnlyList<AnswerDto> Answers);

/// <summary>Answer projection. <see cref="IsCorrect"/> is only populated for authors/trainers.</summary>
public record AnswerDto(Guid Id, string Text, bool? IsCorrect);

public record CreateLessonRequest(string Title, int Order, string? Content, string? VideoUrl);

public record CreateExerciseRequest(string Title, int Order, string? Instructions, string? ExpectedOutcome);

public record CreateQuizRequest(
    string Title,
    int Order,
    int PassingScore,
    int? DurationMinutes,
    IReadOnlyList<CreateQuestionRequest> Questions);

public record CreateExamRequest(
    string Title,
    int Order,
    int PassingScore,
    int? DurationMinutes,
    IReadOnlyList<CreateQuestionRequest> Questions);

public record CreateQuestionRequest(
    string Text,
    QuestionType Type,
    int Points,
    IReadOnlyList<CreateAnswerRequest> Answers);

public record CreateAnswerRequest(string Text, bool IsCorrect);
