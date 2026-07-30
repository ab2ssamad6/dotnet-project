using FluentAssertions;
using Lms.Application.Dtos.Activities;
using Lms.Application.Dtos.Auth;
using Lms.Application.Validation;
using Lms.Domain.Enums;

namespace Lms.Unittests;

public class ValidatorTests
{
    [Fact]
    public void Register_rejects_weak_password_and_mismatch()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("Jane", "Doe", "jane@lms.local", "weak", "nope", "Student");

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Password));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.ConfirmPassword));
    }

    [Fact]
    public void Register_accepts_strong_valid_request()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("Jane", "Doe", "jane@lms.local", "Str0ng#Pass", "Str0ng#Pass", "Student");

        validator.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Register_rejects_unknown_role()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest("A", "B", "a@lms.local", "Str0ng#Pass", "Str0ng#Pass", "Superuser");

        validator.Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Quiz_question_requires_a_correct_answer()
    {
        var validator = new CreateQuestionRequestValidator();
        var question = new CreateQuestionRequest(
            "Pick one",
            QuestionType.MultipleChoice,
            1,
            new List<CreateAnswerRequest>
            {
                new("A", false),
                new("B", false)
            });

        var result = validator.Validate(question);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("correct"));
    }

    [Fact]
    public void TrueFalse_question_requires_exactly_two_answers()
    {
        var validator = new CreateQuestionRequestValidator();
        var question = new CreateQuestionRequest(
            "The sky is blue",
            QuestionType.TrueFalse,
            1,
            new List<CreateAnswerRequest>
            {
                new("True", true),
                new("False", false),
                new("Maybe", false)
            });

        validator.Validate(question).IsValid.Should().BeFalse();
    }
}
