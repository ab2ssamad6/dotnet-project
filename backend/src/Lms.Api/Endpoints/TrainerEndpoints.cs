using Lms.Api.Extensions;
using Lms.Api.Filters;
using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Trainers;

namespace Lms.Api.Endpoints;

public static class TrainerEndpoints
{
    public static IEndpointRouteBuilder MapTrainerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/trainers").WithTags("Trainers");

        group.MapGet("/", async (ITrainerService service, CancellationToken ct,
                int page = 1, int pageSize = 20, string? search = null) =>
                (await service.GetPagedAsync(PagedQuery.Of(page, pageSize, search), ct)).ToHttpResult())
            .RequireAuthorization()
            .WithSummary("List trainers (paged).");

        group.MapGet("/{id:guid}", async (Guid id, ITrainerService service, CancellationToken ct) =>
                (await service.GetByIdAsync(id, ct)).ToHttpResult())
            .RequireAuthorization()
            .WithSummary("Get a trainer by id.");

        group.MapPost("/", async (CreateTrainerRequest request, ITrainerService service, CancellationToken ct) =>
                (await service.CreateAsync(request, ct)).ToCreatedResult(t => $"/api/trainers/{t.Id}"))
            .WithValidation<RouteHandlerBuilder, CreateTrainerRequest>()
            .RequireAuthorization(Roles.Administrator)
            .WithSummary("Create a trainer (Admin).");

        group.MapPut("/{id:guid}", async (Guid id, UpdateTrainerRequest request, ITrainerService service, CancellationToken ct) =>
                (await service.UpdateAsync(id, request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, UpdateTrainerRequest>()
            .RequireAuthorization("ContentManager")
            .WithSummary("Update a trainer (Admin/Trainer).");

        group.MapDelete("/{id:guid}", async (Guid id, ITrainerService service, CancellationToken ct) =>
                (await service.DeleteAsync(id, ct)).ToHttpResult())
            .RequireAuthorization(Roles.Administrator)
            .WithSummary("Delete a trainer (Admin).");

        return app;
    }
}
