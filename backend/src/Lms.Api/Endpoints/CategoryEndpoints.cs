using Lms.Api.Extensions;
using Lms.Api.Filters;
using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Categories;

namespace Lms.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", async (ICategoryService service, CancellationToken ct,
                int page = 1, int pageSize = 20, string? search = null) =>
                (await service.GetPagedAsync(PagedQuery.Of(page, pageSize, search), ct)).ToHttpResult())
            .RequireAuthorization()
            .WithSummary("List categories (paged).");

        group.MapGet("/{id:guid}", async (Guid id, ICategoryService service, CancellationToken ct) =>
                (await service.GetByIdAsync(id, ct)).ToHttpResult())
            .RequireAuthorization()
            .WithSummary("Get a category by id.");

        group.MapPost("/", async (CreateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
                (await service.CreateAsync(request, ct)).ToCreatedResult(c => $"/api/categories/{c.Id}"))
            .WithValidation<RouteHandlerBuilder, CreateCategoryRequest>()
            .RequireAuthorization("ContentManager")
            .WithSummary("Create a category (Admin/Trainer).");

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
                (await service.UpdateAsync(id, request, ct)).ToHttpResult())
            .WithValidation<RouteHandlerBuilder, UpdateCategoryRequest>()
            .RequireAuthorization("ContentManager")
            .WithSummary("Update a category (Admin/Trainer).");

        group.MapDelete("/{id:guid}", async (Guid id, ICategoryService service, CancellationToken ct) =>
                (await service.DeleteAsync(id, ct)).ToHttpResult())
            .RequireAuthorization(Roles.Administrator)
            .WithSummary("Delete a category (Admin).");

        return app;
    }
}
