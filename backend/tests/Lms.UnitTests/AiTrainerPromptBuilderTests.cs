using FluentAssertions;
using Lms.Infrastructure.Services.Anam;

namespace Lms.Unittests;

public class AiTrainerPromptBuilderTests
{
    private const string BasePrompt = "You are Cara, a helpful AI training assistant.";

    private static AiSubjectContext MachineLearningFoundations(Guid? focusModuleId = null, Guid? moduleId = null)
    {
        var module = new AiModuleContext(
            moduleId ?? Guid.NewGuid(),
            "What is ML?",
            "How machines learn from data.",
            1,
            45,
            new[] { "Lesson: Overview" });

        return new AiSubjectContext(
            Guid.NewGuid(),
            "Machine Learning Foundations",
            "Core concepts of supervised and unsupervised learning.",
            "Data & AI",
            "Beginner",
            360,
            "Tina Trainer",
            new[] { module },
            focusModuleId);
    }

    [Fact]
    public void Unscoped_session_keeps_the_configured_prompt_untouched()
    {
        // Preserves the behaviour of the anonymous /api/session-token alias.
        AiTrainerPromptBuilder.Build(BasePrompt, "Cara", subject: null).Should().Be(BasePrompt);
    }

    [Fact]
    public void Prompt_makes_the_persona_a_tutor_of_the_selected_training()
    {
        var prompt = AiTrainerPromptBuilder.Build(BasePrompt, "Cara", MachineLearningFoundations());

        prompt.Should().Contain("You are Cara, an expert tutor for the training \"Machine Learning Foundations\".");
        // The configured prompt is kept as the tone preamble.
        prompt.Should().Contain(BasePrompt);
        // It is told to introduce itself by subject on the first turn.
        prompt.Should().Contain("introduce yourself by name");
        prompt.Should().Contain("Machine Learning Foundations");
    }

    [Fact]
    public void Prompt_includes_the_subject_facts_and_curriculum()
    {
        var prompt = AiTrainerPromptBuilder.Build(BasePrompt, "Cara", MachineLearningFoundations());

        prompt.Should().Contain("Core concepts of supervised and unsupervised learning.");
        prompt.Should().Contain("Data & AI");
        prompt.Should().Contain("Beginner");
        prompt.Should().Contain("Tina Trainer");
        prompt.Should().Contain("1. What is ML? (45 min)");
        prompt.Should().Contain("Lesson: Overview");
    }

    [Fact]
    public void Prompt_guards_the_subject_scope_and_assessment_answers()
    {
        var prompt = AiTrainerPromptBuilder.Build(BasePrompt, "Cara", MachineLearningFoundations());

        prompt.Should().Contain("Stay within the scope of \"Machine Learning Foundations\".");
        prompt.Should().Contain("Never reveal the answers to quizzes or exams");
    }

    [Fact]
    public void Focus_module_wording_appears_only_when_a_module_is_selected()
    {
        var moduleId = Guid.NewGuid();

        var wholeTraining = AiTrainerPromptBuilder.Build(BasePrompt, "Cara", MachineLearningFoundations(moduleId: moduleId));
        wholeTraining.Should().Contain("Cover the training as a whole.");
        wholeTraining.Should().NotContain("This session is focused on module");

        var scopedToModule = AiTrainerPromptBuilder.Build(
            BasePrompt, "Cara", MachineLearningFoundations(focusModuleId: moduleId, moduleId: moduleId));
        scopedToModule.Should().Contain("This session is focused on module 1, \"What is ML?\".");
        scopedToModule.Should().Contain("<- current focus");
        scopedToModule.Should().NotContain("Cover the training as a whole.");
    }

    [Fact]
    public void Persona_name_falls_back_when_blank()
    {
        var prompt = AiTrainerPromptBuilder.Build(BasePrompt, "   ", MachineLearningFoundations());

        prompt.Should().Contain("You are the AI trainer, an expert tutor for the training");
    }

    [Fact]
    public void Long_descriptions_are_truncated_to_bound_the_prompt()
    {
        var subject = MachineLearningFoundations() with { Description = new string('x', 900) };

        var prompt = AiTrainerPromptBuilder.Build(BasePrompt, "Cara", subject);

        prompt.Should().Contain("…");
        prompt.Should().NotContain(new string('x', 500));
    }
}
