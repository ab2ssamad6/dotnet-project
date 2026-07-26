using Lms.Api.Extensions;
using Lms.Api.Filters;
using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Modules;

namespace Lms.Api.Endpoints;

public static class ModuleEndpoints
{
    public static IEndpointRouteBuilder MapModuleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/trainings/{trainingId:guid}/modules",
                async (Guid trainingId, IModuleService service, CancellationToken ct) =>
                    (await service.GetByTrainingAsync(trainingId, ct)).ToHttpResult())
            .WithTags("Modules")
            .RequireAuthorization()
            .WithSummary("List the modules of a training (ordered).");

        var group = app.MapGroup("/api/modules").WithTags("Modules");

        group.MapGet("/{id:guid}", async (Guid id, IModuleService service, CancellationToken ct) =>
                (await service.GetByIdAsync(id, ct)).ToHttpResult())
            .RequireAuthorization()
            .WithSummary("Get a module by id.");

        group.MapPost("/", async (CreateModuleRequest request, IModuleService service, CancellationToken ct) =>
                (await service.CreateAsync(request, ct)).ToCreatedResult(m => $"/api/modules/{m.Id}"))
            .WithValidation<RouteHandlerBuilder, CreateModuleRequest>()
            .RequireAuthorization("ContentManager")
            .WithSummary("Create a module (Admin/Trainer).");

        group.MapPut("/{id:guid}", async (Guid id, UpdateModuleRequest request, IModuleService service, CancellationToken ct) =>
                (await service.UpdateAsync(id, request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, UpdateModuleRequest>()
            .RequireAuthorization("ContentManager")
            .WithSummary("Update a module (Admin/Trainer).");

        group.MapDelete("/{id:guid}", async (Guid id, IModuleService service, CancellationToken ct) =>
                (await service.DeleteAsync(id, ct)).ToHttpResult())
            .RequireAuthorization("ContentManager")
            .WithSummary("Delete a module (Admin/Trainer).");

        return app;
    }
}
