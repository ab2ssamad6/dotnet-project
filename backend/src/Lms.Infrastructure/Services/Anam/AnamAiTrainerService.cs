using System.Net.Http.Json;
using Lms.Application.Abstractions;
using Lms.Application.Common;
using Lms.Application.Dtos.AiTrainer;
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
        if (!_options.IsConfigured)
            return Result<StartSessionResponse>.Failure("AI trainer provider is not configured (missing API key).");

        var personaConfig = new AnamPersonaConfig(
            Name: request.PersonaName ?? _options.PersonaName,
            AvatarId: _options.AvatarId,
            AvatarModel: _options.AvatarModel,
            VoiceId: _options.VoiceId,
            LlmId: _options.LlmId,
            SystemPrompt: _options.SystemPrompt);

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
                new StartSessionResponse(payload.SessionToken, "Anam.ai", request.ModuleId, DateTime.UtcNow));
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
        var module = await _context.Modules.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.ModuleId, cancellationToken);
        if (module is null)
            return Result<ModulePresentationResponse>.NotFound("Module not found.");

        var presentation =
            $"Welcome to \"{module.Title}\". {module.Description} " +
            "This presentation script can be fed to the AI avatar to narrate the module.";
        return Result<ModulePresentationResponse>.Success(
            new ModulePresentationResponse(module.Id, presentation, Live: false));
    }

    public Task<Result> StopSessionAsync(StopSessionRequest request, CancellationToken cancellationToken = default)
    {
        // Sessions are torn down client-side when streaming stops; nothing to revoke server-side.
        _logger.LogInformation("AI trainer session stop requested.");
        return Task.FromResult(Result.Success());
    }
}
