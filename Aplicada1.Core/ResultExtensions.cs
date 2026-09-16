namespace Aplicada1.Core;

public static class ResultExtensions
{
    public static Result ToResult(this bool condition, Error error)
    {
        return condition ? Result.Success() : Result.Failure(error);
    }

    public static Result<T> ToSuccess<T>(this T value) => Result.Success(value);

    public static Result<T> ToFailure<T>(this Error error) => Result.Failure<T>(error);
}