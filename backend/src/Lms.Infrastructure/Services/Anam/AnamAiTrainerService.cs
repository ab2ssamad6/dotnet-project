using System.Net.Http.Json;
using System.Text;
using Lms.Application.Abstractions;
using Lms.Application.Common;
using Lms.Application.Dtos.AiTrainer;
using Lms.Domain.Entities;
using Lms.Infrastructure.Options;
using Lms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lms.Infrastructure.Services.Anam;

/// <summary>
/// Anam.ai implementation of <see cref="IAITrainerService"/>.
/// <para>
/// <see cref="StartSessionAsync"/> is wired to the real provider: it exchanges the configured
/// API key for a short-lived session token that the browser SDK streams with (the same flow as
/// the original Node prototype's <c>/api/session-token</c>). The remaining methods are defined
/// abstractions — Anam.ai drives Q&amp;A/presentation through the client-side realtime SDK rather
/// than documented server endpoints, so they return structured, non-live responses until a
/// server-side provider surface exists.
/// </para>
/// </summary>
public class AnamAiTrainerService : IAITrainerService
{
    public const string HttpClientName = "AnamAi";

    /// <summary>Caps on the curriculum embedded in the system prompt, to bound its length.</summary>
    private const int MaxModules = 20;
    private const int MaxActivitiesPerModule = 12;

    private readonly HttpClient _httpClient;
    private readonly AnamOptions _options;
    private readonly LmsDbContext _context;
    private readonly ILogger<AnamAiTrainerService> _logger;

    public AnamAiTrainerService(
        HttpClient httpClient,
        IOptions<AnamOptions> options,
        LmsDbContext context,
        ILogger<AnamAiTrainerService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<StartSessionResponse>> StartSessionAsync(StartSessionRequest request, CancellationToken cancellationToken = default)
    {
        var personaName = request.PersonaName ?? _options.PersonaName;

        // Resolve the subject before the configuration guard so a bad training/module id fails
        // deterministically even when no provider key is present (e.g. in the Testing environment).
        var subject = await LoadSubjectAsync(request.TrainingId, request.ModuleId, cancellationToken);
        if ((request.TrainingId.HasValue || request.ModuleId.HasValue) && subject is null)
            return Result<StartSessionResponse>.NotFound("Training or module not found.");

        if (!_options.IsConfigured)
            return Result<StartSessionResponse>.Failure("AI trainer provider is not configured (missing API key).");

        var systemPrompt = AiTrainerPromptBuilder.Build(_options.SystemPrompt, personaName, subject);
        _logger.LogDebug("AI trainer system prompt for subject {Subject}:\n{Prompt}", subject?.Title ?? "(none)", systemPrompt);

        var personaConfig = new AnamPersonaConfig(
            Name: personaName,
            AvatarId: _options.AvatarId,
            AvatarModel: _options.AvatarModel,
            VoiceId: _options.VoiceId,
            LlmId: _options.LlmId,
            SystemPrompt: systemPrompt);

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "/v1/auth/session-token",
                new AnamSessionTokenRequest(personaConfig),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Anam.ai session-token request failed with {Status}", response.StatusCode);
                return Result<StartSessionResponse>.Failure("Failed to start AI trainer session.");
            }

            var payload = await response.Content.ReadFromJsonAsync<AnamSessionTokenResponse>(cancellationToken);
            if (payload?.SessionToken is null)
                return Result<StartSessionResponse>.Failure("AI trainer provider returned no session token.");

            return Result<StartSessionResponse>.Success(
                new StartSessionResponse(
                    payload.SessionToken,
                    "Anam.ai",
                    request.ModuleId,
                    DateTime.UtcNow,
                    subject?.TrainingId,
                    subject?.Title,
                    personaName));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error contacting Anam.ai");
            return Result<StartSessionResponse>.Failure("AI trainer provider is unavailable.");
        }
    }

    public Task<Result<AskQuestionResponse>> AskQuestionAsync(AskQuestionRequest request, CancellationToken cancellationToken = default)
    {
        // Anam.ai handles conversational turns over the realtime SDK, not a server REST endpoint.
        var answer = $"[AI trainer] Received: \"{request.Question}\". " +
                     "Conversational replies are delivered through the realtime avatar session.";
        return Task.FromResult(Result<AskQuestionResponse>.Success(new AskQuestionResponse(answer, Live: false)));
    }

    public async Task<Result<ModulePresentationResponse>> GetModulePresentationAsync(ModulePresentationRequest request, CancellationToken cancellationToken = default)
    {
        var subject = await LoadSubjectAsync(trainingId: null, moduleId: request.ModuleId, cancellationToken);
        var module = subject?.FocusModule;
        if (subject is null || module is null)
            return Result<ModulePresentationResponse>.NotFound("Module not found.");

        var sb = new StringBuilder();
        sb.Append($"Welcome to \"{module.Title}\", module {module.Order} of \"{subject.Title}\".");
        if (!string.IsNullOrWhiteSpace(module.Description))
            sb.Append(' ').Append(module.Description!.Trim());
        if (module.Activities.Count > 0)
            sb.Append(" In this module we cover: ").Append(string.Join("; ", module.Activities)).Append('.');

        return Result<ModulePresentationResponse>.Success(
            new ModulePresentationResponse(module.Id, sb.ToString(), Live: false));
    }

    /// <summary>
    /// Loads the training (with its modules and activities) that a session is scoped to.
    /// A <paramref name="moduleId"/> wins over <paramref name="trainingId"/>: its parent training is
    /// loaded and the module itself becomes the focus. Returns null when neither is supplied, or when
    /// the requested training/module does not exist.
    /// </summary>
    private async Task<AiSubjectContext?> LoadSubjectAsync(Guid? trainingId, Guid? moduleId, CancellationToken cancellationToken)
    {
        var resolvedTrainingId = trainingId;
        Guid? focusModuleId = null;

        if (moduleId.HasValue)
        {
            var owner = await _context.Modules.AsNoTracking()
                .Where(m => m.Id == moduleId.Value)
                .Select(m => (Guid?)m.TrainingId)
                .FirstOrDefaultAsync(cancellationToken);

            if (owner is null)
                return null;

            resolvedTrainingId = owner;
            focusModuleId = moduleId;
        }

        if (resolvedTrainingId is null)
            return null;

        // Order inside the projection (never OrderBy a member of an already-projected DTO). The
        // activity kind is derived with type checks, which EF translates to discriminator predicates —
        // safer than naming the TPH discriminator column, and ActivityType itself is an ignored
        // computed CLR property so it cannot be projected.
        var training = await _context.Trainings.AsNoTracking()
            .Where(t => t.Id == resolvedTrainingId.Value)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.Difficulty,
                t.Duration,
                CategoryName = t.Category!.Name,
                TrainerName = t.Trainer!.FirstName + " " + t.Trainer.LastName,
                Modules = t.Modules
                    .OrderBy(m => m.Order)
                    .Select(m => new
                    {
                        m.Id,
                        m.Title,
                        m.Description,
                        m.Order,
                        m.Duration,
                        Activities = m.Activities
                            .OrderBy(a => a.Order)
                            .Select(a => new
                            {
                                Kind = a is Lesson ? "Lesson"
                                    : a is Exercise ? "Exercise"
                                    : a is Quiz ? "Quiz"
                                    : "Exam",
                                a.Title,
                            })
                            .ToList(),
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (training is null)
            return null;

        // Cap in memory: bounding the prompt does not warrant nested SQL paging.
        var modules = training.Modules
            .Take(MaxModules)
            .Select(m => new AiModuleContext(
                m.Id,
                m.Title,
                m.Description,
                m.Order,
                m.Duration,
                m.Activities.Take(MaxActivitiesPerModule).Select(a => $"{a.Kind}: {a.Title}").ToList()))
            .ToList();

        return new AiSubjectContext(
            training.Id,
            training.Title,
            training.Description,
            training.CategoryName,
            training.Difficulty.ToString(),
            training.Duration,
            string.IsNullOrWhiteSpace(training.TrainerName) ? null : training.TrainerName.Trim(),
            modules,
            focusModuleId);
    }

    public Task<Result> StopSessionAsync(StopSessionRequest request, CancellationToken cancellationToken = default)
    {
        // Sessions are torn down client-side when streaming stops; nothing to revoke server-side.
        _logger.LogInformation("AI trainer session stop requested.");
        return Task.FromResult(Result.Success());
    }
}
