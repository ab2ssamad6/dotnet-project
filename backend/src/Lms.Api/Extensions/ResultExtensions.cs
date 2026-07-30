using Lms.Application.Common;

namespace Lms.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess ? Results.NoContent() : Problem(result);

    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.IsSuccess ? Results.Ok(result.Value) : Problem(result);

    public static IResult ToCreatedResult<T>(this Result<T> result, Func<T, string> location) =>
        result.IsSuccess ? Results.Created(location(result.Value), result.Value) : Problem(result);

    private static IResult Problem(Result result)
    {
        var status = result.ErrorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            detail: result.Error,
            statusCode: status,
            title: result.ErrorType.ToString());
    }
}
