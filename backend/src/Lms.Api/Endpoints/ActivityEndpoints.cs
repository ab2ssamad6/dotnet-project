using Lms.Api.Extensions;
using Lms.Api.Filters;
using Lms.Application.Abstractions;
using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Activities;

namespace Lms.Api.Endpoints;

public static class ActivityEndpoints
{
    public static IEndpointRouteBuilder MapActivityEndpoints(this IEndpointRouteBuilder app)
    {
        static bool CanSeeAnswers(ICurrentUser user) =>
            user.IsInRole(Roles.Administrator) || user.IsInRole(Roles.Trainer);

        app.MapGet("/api/modules/{moduleId:guid}/activities",
                async (Guid moduleId, IActivityService service, ICurrentUser user, CancellationToken ct) =>
                    (await service.GetByModuleAsync(moduleId, CanSeeAnswers(user), ct)).ToHttpResult())
            .WithTags("Activities")
            .RequireAuthorization()
            .WithSummary("List the learning activities of a module.");

        var group = app.MapGroup("/api").WithTags("Activities");

        group.MapGet("/activities/{id:guid}",
                async (Guid id, IActivityService service, ICurrentUser user, CancellationToken ct) =>
                    (await service.GetByIdAsync(id, CanSeeAnswers(user), ct)).ToHttpResult())
            .RequireAuthorization()
            .WithSummary("Get a learning activity by id.");

        group.MapPost("/modules/{moduleId:guid}/lessons",
                async (Guid moduleId, CreateLessonRequest request, IActivityService service, CancellationToken ct) =>
                    (await service.CreateLessonAsync(moduleId, request, ct)).ToCreatedResult(a => $"/api/activities/{a.Id}"))
            .WithValidation<RouteHandlerBuilder, CreateLessonRequest>()
            .RequireAuthorization("ContentManager")
            .WithSummary("Add a lesson to a module (Admin/Trainer).");

        group.MapPost("/modules/{moduleId:guid}/exercises",
                async (Guid moduleId, CreateExerciseRequest request, IActivityService service, CancellationToken ct) =>
                    (await service.CreateExerciseAsync(moduleId, request, ct)).ToCreatedResult(a => $"/api/activities/{a.Id}"))
            .WithValidation<RouteHandlerBuilder, CreateExerciseRequest>()
            .RequireAuthorization("ContentManager")
            .WithSummary("Add an exercise to a module (Admin/Trainer).");

        group.MapPost("/modules/{moduleId:guid}/quizzes",
                async (Guid moduleId, CreateQuizRequest request, IActivityService service, CancellationToken ct) =>
                    (await service.CreateQuizAsync(moduleId, request, ct)).ToCreatedResult(a => $"/api/activities/{a.Id}"))
            .WithValidation<RouteHandlerBuilder, CreateQuizRequest>()
            .RequireAuthorization("ContentManager")
            .WithSummary("Add a quiz to a module (Admin/Trainer).");

        group.MapPost("/modules/{moduleId:guid}/exams",
                async (Guid moduleId, CreateExamRequest request, IActivityService service, CancellationToken ct) =>
                    (await service.CreateExamAsync(moduleId, request, ct)).ToCreatedResult(a => $"/api/activities/{a.Id}"))
            .WithValidation<RouteHandlerBuilder, CreateExamRequest>()
            .RequireAuthorization("ContentManager")
            .WithSummary("Add an exam to a module (Admin/Trainer).");

        group.MapDelete("/activities/{id:guid}",
                async (Guid id, IActivityService service, CancellationToken ct) =>
                    (await service.DeleteAsync(id, ct)).ToHttpResult())
            .RequireAuthorization("ContentManager")
            .WithSummary("Delete a learning activity (Admin/Trainer).");

        return app;
    }
}
