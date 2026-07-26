using Lms.Api.Extensions;
using Lms.Api.Filters;
using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Trainings;

namespace Lms.Api.Endpoints;

public static class TrainingEndpoints
{
    public static IEndpointRouteBuilder MapTrainingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/trainings").WithTags("Trainings");

        group.MapGet("/", async (ITrainingService service, CancellationToken ct,
                int page = 1, int pageSize = 20, string? search = null) =>
                (await service.GetPagedAsync(PagedQuery.Of(page, pageSize, search), onlyPublished: false, ct)).ToHttpResult())
            .RequireAuthorization()
            .WithSummary("List all trainings (paged).");

        group.MapGet("/{id:guid}", async (Guid id, ITrainingService service, CancellationToken ct) =>
                (await service.GetByIdAsync(id, ct)).ToHttpResult())
            .RequireAuthorization()
            .WithSummary("Get a training by id.");

        group.MapPost("/", async (CreateTrainingRequest request, ITrainingService service, CancellationToken ct) =>
                (await service.CreateAsync(request, ct)).ToCreatedResult(t => $"/api/trainings/{t.Id}"))
            .WithValidation<RouteHandlerBuilder, CreateTrainingRequest>()
            .RequireAuthorization("ContentManager")
            .WithSummary("Create a training (Admin/Trainer).");

        group.MapPut("/{id:guid}", async (Guid id, UpdateTrainingRequest request, ITrainingService service, CancellationToken ct) =>
                (await service.UpdateAsync(id, request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, UpdateTrainingRequest>()
            .RequireAuthorization("ContentManager")
            .WithSummary("Update a training (Admin/Trainer).");

        group.MapDelete("/{id:guid}", async (Guid id, ITrainingService service, CancellationToken ct) =>
                (await service.DeleteAsync(id, ct)).ToHttpResult())
            .RequireAuthorization("ContentManager")
            .WithSummary("Delete a training (Admin/Trainer).");

        return app;
    }
}
