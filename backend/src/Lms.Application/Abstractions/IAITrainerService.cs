using Lms.Application.Common;
using Lms.Application.Dtos.AiTrainer;

namespace Lms.Application.Abstractions;

/// <summary>
/// Abstraction over an external AI avatar trainer provider (Anam.ai). The concrete
/// implementation talks to the provider over REST via <c>IHttpClientFactory</c>.
/// </summary>
public interface IAITrainerService
{
    /// <summary>Opens a session and returns a token the browser SDK can stream with.</summary>
    Task<Result<StartSessionResponse>> StartSessionAsync(StartSessionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Asks the avatar a question within an open session.</summary>
    Task<Result<AskQuestionResponse>> AskQuestionAsync(AskQuestionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Produces a spoken presentation script for a module.</summary>
    Task<Result<ModulePresentationResponse>> GetModulePresentationAsync(ModulePresentationRequest request, CancellationToken cancellationToken = default);

    /// <summary>Tears down an open session.</summary>
    Task<Result> StopSessionAsync(StopSessionRequest request, CancellationToken cancellationToken = default);
}
