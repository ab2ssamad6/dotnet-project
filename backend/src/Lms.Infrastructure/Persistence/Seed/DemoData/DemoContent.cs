using Lms.Domain.Entities;
using Lms.Domain.Enums;

namespace Lms.Infrastructure.Persistence.Seed.DemoData;

/// <summary>
/// Small factory helpers shared by the demo course definitions. They assign the 1-based
/// <c>Order</c> of modules and activities from their position, so the course files never
/// have to number anything by hand.
/// </summary>
/// <remarks>
/// Lesson text is rendered by the frontend inside a <c>whitespace-pre-line</c> paragraph
/// (see <c>frontend/src/pages/student/LearningPage.tsx</c>), so it must be plain text:
/// blank lines separate paragraphs and "-" starts a bullet. Markdown is not rendered.
/// </remarks>
public static class DemoContent
{
    public static Training NewTraining(
        string title,
        string description,
        DifficultyLevel difficulty,
        Category category,
        Trainer trainer,
        params Module[] modules)
    {
        for (var i = 0; i < modules.Length; i++)
            modules[i].Order = i + 1;

        return new Training
        {
            Title = title,
            Description = description,
            Difficulty = difficulty,
            Duration = modules.Sum(m => m.Duration),
            Status = TrainingStatus.Published,
            Published = true,
            Category = category,
            Trainer = trainer,
            Modules = modules.ToList()
        };
    }

    public static Module NewModule(
        string title,
        string description,
        int duration,
        bool aiAvatarEnabled,
        params LearningActivity[] activities)
    {
        for (var i = 0; i < activities.Length; i++)
            activities[i].Order = i + 1;

        return new Module
        {
            Title = title,
            Description = description,
            Duration = duration,
            AiAvatarEnabled = aiAvatarEnabled,
            Activities = activities.ToList()
        };
    }

    public static Lesson NewLesson(string title, string content) =>
        new() { Title = title, Content = content };

    public static Exercise NewExercise(string title, string instructions, string expectedOutcome) =>
        new() { Title = title, Instructions = instructions, ExpectedOutcome = expectedOutcome };

    public static Quiz NewQuiz(string title, params Question[] questions) =>
        new() { Title = title, PassingScore = 60, DurationMinutes = 10, Questions = questions.ToList() };

    public static Exam NewExam(string title, params Question[] questions) =>
        new() { Title = title, PassingScore = 70, DurationMinutes = 45, Questions = questions.ToList() };

    /// <summary>Single-correct-answer question.</summary>
    public static Question Choice(string text, params (string Text, bool IsCorrect)[] answers) =>
        Build(text, QuestionType.MultipleChoice, 1, answers);

    /// <summary>Question where several answers are correct.</summary>
    public static Question Multiple(string text, params (string Text, bool IsCorrect)[] answers) =>
        Build(text, QuestionType.MultipleAnswers, 2, answers);

    public static Question TrueFalse(string text, bool correct) =>
        Build(text, QuestionType.TrueFalse, 1, new[] { ("True", correct), ("False", !correct) });

    /// <summary>Overrides the default point value, e.g. for exam questions.</summary>
    public static Question Worth(this Question question, int points)
    {
        question.Points = points;
        return question;
    }

    private static Question Build(
        string text,
        QuestionType type,
        int points,
        (string Text, bool IsCorrect)[] answers) =>
        new()
        {
            Text = text,
            Type = type,
            Points = points,
            Answers = answers
                .Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect })
                .ToList()
        };
}
