using FluentValidation;
using Lms.Application.Dtos.Activities;
using Lms.Domain.Enums;

namespace Lms.Application.Validation;

public class CreateLessonRequestValidator : AbstractValidator<CreateLessonRequest>
{
    public CreateLessonRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
    }
}

public class CreateExerciseRequestValidator : AbstractValidator<CreateExerciseRequest>
{
    public CreateExerciseRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
    }
}

public class CreateAnswerRequestValidator : AbstractValidator<CreateAnswerRequest>
{
    public CreateAnswerRequestValidator() => RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
}

public class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
{
    public CreateQuestionRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Points).GreaterThan(0);
        RuleFor(x => x.Answers).NotEmpty().WithMessage("A question must have at least two answers.")
            .Must(a => a.Count >= 2).WithMessage("A question must have at least two answers.");
        RuleForEach(x => x.Answers).SetValidator(new CreateAnswerRequestValidator());
        RuleFor(x => x.Answers)
            .Must(a => a.Any(ans => ans.IsCorrect))
            .WithMessage("At least one answer must be marked correct.");
        RuleFor(x => x)
            .Must(q => q.Type != QuestionType.TrueFalse || q.Answers.Count == 2)
            .WithMessage("True/False questions must have exactly two answers.");
        RuleFor(x => x)
            .Must(q => q.Type != QuestionType.MultipleChoice || q.Answers.Count(a => a.IsCorrect) == 1)
            .WithMessage("Multiple-choice questions must have exactly one correct answer.");
        RuleFor(x => x)
            .Must(q => q.Type != QuestionType.TrueFalse || q.Answers.Count(a => a.IsCorrect) == 1)
            .WithMessage("True/False questions must have exactly one correct answer.");
    }
}

public class CreateQuizRequestValidator : AbstractValidator<CreateQuizRequest>
{
    public CreateQuizRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PassingScore).InclusiveBetween(0, 100);
        RuleFor(x => x.DurationMinutes).GreaterThan(0).When(x => x.DurationMinutes.HasValue);
        RuleFor(x => x.Questions).NotEmpty().WithMessage("A quiz must contain at least one question.");
        RuleForEach(x => x.Questions).SetValidator(new CreateQuestionRequestValidator());
    }
}

public class CreateExamRequestValidator : AbstractValidator<CreateExamRequest>
{
    public CreateExamRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PassingScore).InclusiveBetween(0, 100);
        RuleFor(x => x.DurationMinutes).GreaterThan(0).When(x => x.DurationMinutes.HasValue);
        RuleFor(x => x.Questions).NotEmpty().WithMessage("An exam must contain at least one question.");
        RuleForEach(x => x.Questions).SetValidator(new CreateQuestionRequestValidator());
    }
}
