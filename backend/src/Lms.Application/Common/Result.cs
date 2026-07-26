namespace Lms.Application.Common;

/// <summary>Classifies a failed <see cref="Result"/> so the API layer can pick an HTTP status.</summary>
public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    Failure = 6
}

/// <summary>Outcome of a service operation without a return value.</summary>
public class Result
{
    protected Result(bool isSuccess, string? error, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public ErrorType ErrorType { get; }

    public static Result Success() => new(true, null, ErrorType.None);
    public static Result Failure(string error, ErrorType type = ErrorType.Failure) => new(false, error, type);

    public static Result NotFound(string error = "Resource not found.") => Failure(error, ErrorType.NotFound);
    public static Result Conflict(string error) => Failure(error, ErrorType.Conflict);
    public static Result Validation(string error) => Failure(error, ErrorType.Validation);
    public static Result Unauthorized(string error = "Unauthorized.") => Failure(error, ErrorType.Unauthorized);
    public static Result Forbidden(string error = "Forbidden.") => Failure(error, ErrorType.Forbidden);
}

/// <summary>Outcome of a service operation carrying a value on success.</summary>
public class Result<T> : Result
{
    private readonly T? _value;

    private Result(T? value, bool isSuccess, string? error, ErrorType errorType)
        : base(isSuccess, error, errorType)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static Result<T> Success(T value) => new(value, true, null, ErrorType.None);

    public static new Result<T> Failure(string error, ErrorType type = ErrorType.Failure) =>
        new(default, false, error, type);

    public static new Result<T> NotFound(string error = "Resource not found.") => Failure(error, ErrorType.NotFound);
    public static new Result<T> Conflict(string error) => Failure(error, ErrorType.Conflict);
    public static new Result<T> Validation(string error) => Failure(error, ErrorType.Validation);
    public static new Result<T> Unauthorized(string error = "Unauthorized.") => Failure(error, ErrorType.Unauthorized);
    public static new Result<T> Forbidden(string error = "Forbidden.") => Failure(error, ErrorType.Forbidden);
}
