using SlotSmart.Shared.Errors;

namespace SlotSmart.Shared.Results;

/// <summary>
/// Outcome of a use case that produces no value. Avoid throwing for expected failures; return a Result.
/// </summary>
public readonly record struct Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    private Result(bool isSuccess, Error error)
    {
        if (isSuccess && !error.IsNone)
        {
            throw new ArgumentException("Successful results must carry Error.None.", nameof(error));
        }

        if (!isSuccess && error.IsNone)
        {
            throw new ArgumentException("Failed results must carry a non-empty Error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Outcome of a use case that produces a value of type <typeparamref name="T"/>.
/// </summary>
public readonly record struct Result<T>
{
    private readonly T? _value;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
        Error = Error.None;
    }

    private Result(Error error)
    {
        _value = default;
        IsSuccess = false;
        Error = error.IsNone
            ? throw new ArgumentException("Failed results must carry a non-empty Error.", nameof(error))
            : error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
