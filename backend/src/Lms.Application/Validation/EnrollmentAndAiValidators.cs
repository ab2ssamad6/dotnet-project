using FluentValidation;
using Lms.Application.Dtos.AiTrainer;
using Lms.Application.Dtos.Enrollments;

namespace Lms.Application.Validation;

public class EnrollRequestValidator : AbstractValidator<EnrollRequest>
{
    public EnrollRequestValidator() => RuleFor(x => x.TrainingId).NotEmpty();
}

public class CompleteModuleRequestValidator : AbstractValidator<CompleteModuleRequest>
{
    public CompleteModuleRequestValidator() => RuleFor(x => x.ModuleId).NotEmpty();
}

public class SubmitQuizRequestValidator : AbstractValidator<SubmitQuizRequest>
{
    public SubmitQuizRequestValidator()
    {
        RuleFor(x => x.ActivityId).NotEmpty();
        RuleFor(x => x.Answers).NotEmpty().WithMessage("Provide at least one answer.");
        RuleForEach(x => x.Answers).ChildRules(a =>
            a.RuleFor(s => s.QuestionId).NotEmpty());
    }
}

public class StartSessionRequestValidator : AbstractValidator<StartSessionRequest>
{
    public StartSessionRequestValidator() => RuleFor(x => x.PersonaName).MaximumLength(100);
}

public class AskQuestionRequestValidator : AbstractValidator<AskQuestionRequest>
{
    public AskQuestionRequestValidator()
    {
        RuleFor(x => x.SessionToken).NotEmpty();
        RuleFor(x => x.Question).NotEmpty().MaximumLength(2000);
    }
}

public class ModulePresentationRequestValidator : AbstractValidator<ModulePresentationRequest>
{
    public ModulePresentationRequestValidator() => RuleFor(x => x.ModuleId).NotEmpty();
}

public class StopSessionRequestValidator : AbstractValidator<StopSessionRequest>
{
    public StopSessionRequestValidator() => RuleFor(x => x.SessionToken).NotEmpty();
}
