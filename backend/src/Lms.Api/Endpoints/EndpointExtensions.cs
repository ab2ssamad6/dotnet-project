namespace Lms.Api.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();
        app.MapCategoryEndpoints();
        app.MapTrainerEndpoints();
        app.MapTrainingEndpoints();
        app.MapModuleEndpoints();
        app.MapActivityEndpoints();
        app.MapEnrollmentEndpoints();
        app.MapDashboardEndpoints();
        app.MapAiTrainerEndpoints();
        return app;
    }
}
