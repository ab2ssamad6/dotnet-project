using Lms.Api.Extensions;
using Lms.Application.Abstractions.Services;
using Lms.Application.Common;

namespace Lms.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/dashboard", async (IDashboardService service, CancellationToken ct) =>
                (await service.GetAsync(ct)).ToHttpResult())
            .WithTags("Administration")
            .RequireAuthorization(Roles.Administrator)
            .WithSummary("Administrator dashboard: counts, breakdowns and recent activity.");

        return app;
    }
}
