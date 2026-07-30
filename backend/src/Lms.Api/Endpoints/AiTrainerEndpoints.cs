using Lms.Api.Extensions;
using Lms.Api.Filters;
using Lms.Application.Abstractions;
using Lms.Application.Dtos.AiTrainer;

namespace Lms.Api.Endpoints;

public static class AiTrainerEndpoints
{
    public static IEndpointRouteBuilder MapAiTrainerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai-trainer").WithTags("AI Trainer");

        group.MapPost("/session", async (StartSessionRequest request, IAITrainerService service, CancellationToken ct) =>
                (await service.StartSessionAsync(request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, StartSessionRequest>()
            .RequireAuthorization()
            .WithSummary("Start an AI trainer avatar session and get a streaming session token.");

        group.MapPost("/ask", async (AskQuestionRequest request, IAITrainerService service, CancellationToken ct) =>
                (await service.AskQuestionAsync(request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, AskQuestionRequest>()
            .RequireAuthorization()
            .WithSummary("Ask the AI trainer a question within a session.");

        group.MapPost("/module-presentation", async (ModulePresentationRequest request, IAITrainerService service, CancellationToken ct) =>
                (await service.GetModulePresentationAsync(request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, ModulePresentationRequest>()
            .RequireAuthorization()
            .WithSummary("Get an AI presentation script for a module.");

        group.MapPost("/stop", async (StopSessionRequest request, IAITrainerService service, CancellationToken ct) =>
                (await service.StopSessionAsync(request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, StopSessionRequest>()
            .RequireAuthorization()
            .WithSummary("Stop an AI trainer session.");

        app.MapPost("/api/session-token", async (IAITrainerService service, CancellationToken ct) =>
            {
                var result = await service.StartSessionAsync(new StartSessionRequest(null, null), ct);
                return result.IsSuccess
                    ? Results.Ok(new { sessionToken = result.Value.SessionToken })
                    : result.ToHttpResult();
            })
            .WithTags("AI Trainer")
            .AllowAnonymous()
            .WithSummary("Compatibility alias returning { sessionToken } for the sample frontend.");

        return app;
    }
}
