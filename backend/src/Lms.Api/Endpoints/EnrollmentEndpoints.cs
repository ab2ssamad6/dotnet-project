using Lms.Api.Extensions;
using Lms.Api.Filters;
using Lms.Application.Abstractions;
using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Enrollments;

namespace Lms.Api.Endpoints;

public static class EnrollmentEndpoints
{
    public static IEndpointRouteBuilder MapEnrollmentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/catalog", async (ITrainingService service, CancellationToken ct,
                int page = 1, int pageSize = 20, string? search = null) =>
                (await service.GetPagedAsync(PagedQuery.Of(page, pageSize, search), onlyPublished: true, ct)).ToHttpResult())
            .WithTags("Enrollment")
            .RequireAuthorization()
            .WithSummary("Browse published trainings.");

        var group = app.MapGroup("/api/enrollments")
            .WithTags("Enrollment")
            .RequireAuthorization(Roles.Student);

        group.MapPost("/", (
                EnrollRequest request, IEnrollmentService service, ICurrentUser user, CancellationToken ct) =>
                Requires(user, id => service.EnrollAsync(id, request, ct), r => r.ToCreatedResult(_ => "/api/enrollments")))
            .WithValidation<RouteHandlerBuilder, EnrollRequest>()
            .WithSummary("Enroll the current student in a training.");

        group.MapGet("/", (IEnrollmentService service, ICurrentUser user, CancellationToken ct) =>
                Requires(user, id => service.GetMyEnrollmentsAsync(id, ct), r => r.ToHttpResult()))
            .WithSummary("List the current student's enrollments.");

        group.MapGet("/{trainingId:guid}/progress", (
                Guid trainingId, IEnrollmentService service, ICurrentUser user, CancellationToken ct) =>
                Requires(user, id => service.GetProgressAsync(id, trainingId, ct), r => r.ToHttpResult()))
            .WithSummary("Get progress for a training.");

        group.MapPost("/{trainingId:guid}/complete-module", (
                Guid trainingId, CompleteModuleRequest request, IEnrollmentService service, ICurrentUser user, CancellationToken ct) =>
                Requires(user, id => service.CompleteModuleAsync(id, trainingId, request, ct), r => r.ToHttpResult()))
            .WithValidation<RouteHandlerBuilder, CompleteModuleRequest>()
            .WithSummary("Mark a module as completed.");

        group.MapPost("/{trainingId:guid}/submit-quiz", (
                Guid trainingId, SubmitQuizRequest request, IEnrollmentService service, ICurrentUser user, CancellationToken ct) =>
                Requires(user, id => service.SubmitQuizAsync(id, trainingId, request, ct), r => r.ToHttpResult()))
            .WithValidation<RouteHandlerBuilder, SubmitQuizRequest>()
            .WithSummary("Submit answers to a quiz or exam and get a graded result.");

        group.MapGet("/{trainingId:guid}/certificate", (
                Guid trainingId, IEnrollmentService service, ICurrentUser user, CancellationToken ct) =>
                Requires(user, id => service.GetCertificateAsync(id, trainingId, ct), r => r.ToHttpResult()))
            .WithSummary("Get certificate availability for a completed training.");

        return app;
    }

    private static async Task<IResult> Requires<T>(
        ICurrentUser user,
        Func<Guid, Task<Result<T>>> operation,
        Func<Result<T>, IResult> toResult)
    {
        if (user.UserId is not { } studentId)
            return Results.Unauthorized();
        return toResult(await operation(studentId));
    }
}
