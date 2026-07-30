using Lms.Application.Common;
using Lms.Application.Dtos.AiTrainer;

namespace Lms.Application.Abstractions;

public interface IAITrainerService
{
    Task<Result<StartSessionResponse>> StartSessionAsync(StartSessionRequest request, CancellationToken cancellationToken = default);

    Task<Result<AskQuestionResponse>> AskQuestionAsync(AskQuestionRequest request, CancellationToken cancellationToken = default);

    Task<Result<ModulePresentationResponse>> GetModulePresentationAsync(ModulePresentationRequest request, CancellationToken cancellationToken = default);

    Task<Result> StopSessionAsync(StopSessionRequest request, CancellationToken cancellationToken = default);
}
