using FluentValidation;
using Lms.Application.Dtos.Categories;
using Lms.Application.Dtos.Modules;
using Lms.Application.Dtos.Trainers;
using Lms.Application.Dtos.Trainings;

namespace Lms.Application.Validation;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public class CreateTrainerRequestValidator : AbstractValidator<CreateTrainerRequest>
{
    public CreateTrainerRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Biography).MaximumLength(2000);
        RuleFor(x => x.Expertise).MaximumLength(500);
        RuleFor(x => x.Phone).MaximumLength(40);
    }
}

public class UpdateTrainerRequestValidator : AbstractValidator<UpdateTrainerRequest>
{
    public UpdateTrainerRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Biography).MaximumLength(2000);
        RuleFor(x => x.Expertise).MaximumLength(500);
        RuleFor(x => x.Phone).MaximumLength(40);
    }
}

public class CreateTrainingRequestValidator : AbstractValidator<CreateTrainingRequest>
{
    public CreateTrainingRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Difficulty).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.TrainerId).NotEmpty();
    }
}

public class UpdateTrainingRequestValidator : AbstractValidator<UpdateTrainingRequest>
{
    public UpdateTrainingRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Difficulty).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.TrainerId).NotEmpty();
    }
}

public class CreateModuleRequestValidator : AbstractValidator<CreateModuleRequest>
{
    public CreateModuleRequestValidator()
    {
        RuleFor(x => x.TrainingId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
    }
}

public class UpdateModuleRequestValidator : AbstractValidator<UpdateModuleRequest>
{
    public UpdateModuleRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
    }
}
