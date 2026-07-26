using FluentValidation;

namespace Lms.Api.Filters;

/// <summary>
/// Minimal-API endpoint filter that validates the first argument of type <typeparamref name="T"/>
/// using the registered FluentValidation validator, returning a 400 problem response on failure.
/// </summary>
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator) => _validator = validator;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var model = context.Arguments.OfType<T>().FirstOrDefault();
        if (model is null)
            return Results.Problem("Invalid request body.", statusCode: StatusCodes.Status400BadRequest);

        var validation = await _validator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}

public static class ValidationFilterExtensions
{
    /// <summary>Attaches a <see cref="ValidationFilter{T}"/> to a route builder.</summary>
    public static TBuilder WithValidation<TBuilder, T>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
        where T : class
        => builder.AddEndpointFilter<TBuilder, ValidationFilter<T>>();
}
