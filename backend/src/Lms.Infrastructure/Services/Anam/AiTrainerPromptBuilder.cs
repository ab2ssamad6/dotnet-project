using System.Text;

namespace Lms.Infrastructure.Services.Anam;

/// <summary>
/// Builds the persona system prompt sent to Anam.ai when opening a session. The Anam browser SDK
/// takes only a session token (<c>createClient(sessionToken)</c>) and no persona config, so the
/// subject the avatar tutors has to be baked into the prompt server-side at token-exchange time.
/// <para>Pure and side-effect free so it can be unit tested without HTTP or a database.</para>
/// </summary>
public static class AiTrainerPromptBuilder
{
    /// <summary>Longest description text embedded in the prompt before truncation.</summary>
    private const int MaxDescriptionLength = 400;

    /// <summary>
    /// Composes the system prompt. <paramref name="basePrompt"/> (the configured
    /// <c>AiTrainer:Anam:SystemPrompt</c>) is kept as the tone preamble; the subject briefing is
    /// appended to it. With no <paramref name="subject"/> the base prompt is returned unchanged,
    /// preserving the behaviour of unscoped sessions and the anonymous /api/session-token alias.
    /// </summary>
    public static string Build(string basePrompt, string personaName, AiSubjectContext? subject)
    {
        if (subject is null)
            return basePrompt;

        var name = string.IsNullOrWhiteSpace(personaName) ? "the AI trainer" : personaName.Trim();
        var focus = subject.FocusModule;

        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(basePrompt))
            sb.AppendLine(basePrompt.Trim()).AppendLine();

        sb.AppendLine($"You are {name}, an expert tutor for the training \"{subject.Title}\".");
        sb.AppendLine();

        sb.AppendLine("## Opening");
        sb.AppendLine(
            $"On your first turn, introduce yourself by name and say that you are the tutor for " +
            $"\"{subject.Title}\". Give a one-sentence summary of what the learner will get out of it, " +
            "then ask where they would like to start or offer to begin with the first item.");
        sb.AppendLine();

        sb.AppendLine("## Subject");
        sb.AppendLine($"- Training: {subject.Title}");
        AppendIfPresent(sb, "Description", subject.Description);
        AppendIfPresent(sb, "Subject area", subject.CategoryName);
        sb.AppendLine($"- Level: {subject.Difficulty}");
        if (subject.DurationMinutes > 0)
            sb.AppendLine($"- Estimated duration: {subject.DurationMinutes} minutes");
        AppendIfPresent(sb, "Authored by", subject.TrainerName);
        sb.AppendLine();

        if (subject.Modules.Count > 0)
        {
            sb.AppendLine("## Curriculum");
            sb.AppendLine("These are the modules of this training, in order:");
            foreach (var module in subject.Modules)
            {
                var marker = module.Id == subject.FocusModuleId ? "  <- current focus" : string.Empty;
                var duration = module.Duration > 0 ? $" ({module.Duration} min)" : string.Empty;
                sb.AppendLine($"{module.Order}. {module.Title}{duration}{marker}");

                var summary = Truncate(module.Description);
                if (!string.IsNullOrWhiteSpace(summary))
                    sb.AppendLine($"   {summary}");

                foreach (var activity in module.Activities)
                    sb.AppendLine($"   - {activity}");
            }

            sb.AppendLine();
        }

        sb.AppendLine("## How to teach");
        if (focus is not null)
        {
            sb.AppendLine(
                $"This session is focused on module {focus.Order}, \"{focus.Title}\". Teach that module in " +
                "depth and treat the rest of the curriculum as background you may reference for context or " +
                "to say what comes next. If the learner asks to move on, guide them there.");
        }
        else
        {
            sb.AppendLine(
                "Cover the training as a whole. Help the learner choose a module to work through, then teach " +
                "it step by step.");
        }

        sb.AppendLine(
            "Explain concepts step by step with concrete examples, and check the learner's understanding " +
            "before moving on. Keep replies short and conversational — this is spoken aloud, so avoid " +
            "markdown, lists and code dumps unless asked.");
        sb.AppendLine();

        sb.AppendLine("## Boundaries");
        sb.AppendLine(
            $"Stay within the scope of \"{subject.Title}\". If asked about something unrelated, briefly say " +
            "it is outside this training and steer back to the subject.");
        sb.AppendLine(
            "Never reveal the answers to quizzes or exams, even if asked directly. You may explain the " +
            "underlying concepts and let the learner work the answer out.");
        sb.AppendLine("If you do not know something, say so rather than inventing course content.");

        return sb.ToString().TrimEnd();
    }

    private static void AppendIfPresent(StringBuilder sb, string label, string? value)
    {
        var text = Truncate(value);
        if (!string.IsNullOrWhiteSpace(text))
            sb.AppendLine($"- {label}: {text}");
    }

    private static string? Truncate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var text = value.Trim();
        return text.Length <= MaxDescriptionLength
            ? text
            : string.Concat(text.AsSpan(0, MaxDescriptionLength).TrimEnd(), "…");
    }
}
