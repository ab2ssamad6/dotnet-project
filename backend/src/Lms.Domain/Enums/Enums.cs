namespace Lms.Domain.Enums;

public enum DifficultyLevel
{
    Beginner = 0,
    Intermediate = 1,
    Advanced = 2,
    Expert = 3
}

public enum TrainingStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

/// <summary>Kind of learning activity, used as the EF Core TPH discriminator.</summary>
public enum ActivityType
{
    Lesson = 0,
    Exercise = 1,
    Quiz = 2,
    Exam = 3
}

public enum QuestionType
{
    MultipleChoice = 0,
    MultipleAnswers = 1,
    TrueFalse = 2
}

public enum EnrollmentStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2
}
