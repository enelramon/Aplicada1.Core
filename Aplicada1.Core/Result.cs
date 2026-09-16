namespace Aplicada1.Core;

public class Result
{
    public Result(bool isSuccess, Error error)
    {
        if ((isSuccess && error != Error.None) ||
            (!isSuccess && error == Error.None))
        {
            throw new InvalidOperationException(
                $"Invalid Result state: isSuccess={isSuccess}, error=({error.Code}, {error.Description}). " +
                "A successful result must use Error.None and a failed result must provide a non-empty error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public Result OnSuccess(Action action)
    {
        if (IsSuccess)
        {
            action();
        }

        return this;
    }

    public Result OnFailure(Action action)
    {
        if (IsFailure)
        {
            action();
        }

        return this;
    }

    public async Task<Result> OnSuccessAsync(Func<Task> action)
    {
        if (IsSuccess)
        {
            await action();
        }

        return this;
    }

    public async Task<Result> OnFailureAsync(Func<Task> action)
    {
        if (IsFailure)
        {
            await action();
        }

        return this;
    }

    public Result Bind(Func<Result> next)
    {
        return IsSuccess ? next() : this;
    }

    public Result<T> Bind<T>(Func<Result<T>> next)
    {
        return IsSuccess ? next() : Result.Failure<T>(Error);
    }

    public Result<T> Bind<T>(Func<T, Result<T>> next)
    {
        return IsSuccess ? next(default!) : Result.Failure<T>(Error);
    }

    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Error);
    }

    public Result Finally(Action action)
    {
        action();
        return this;
    }

    public Result<T> Then<T>(Func<T> onSuccess)
    {
        return IsSuccess ? Result.Success(onSuccess()) : Result.Failure<T>(Error);
    }

    public static Result Try(Func<Result> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            return Failure(new Error("EXCEPTION", ex.Message));
        }
    }

    public static Result<T> Try<T>(Func<T> action)
    {
        try
        {
            return Result.Success(action());
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(new Error("EXCEPTION", ex.Message));
        }
    }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of failed result");

    public T? ValueOrDefault => _value;

    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
        {
            action(Value);
        }

        return this;
    }

    public Result<T> OnFailure(Action<Error> action)
    {
        if (IsFailure)
        {
            action(Error);
        }

        return this;
    }

    public async Task<Result<T>> OnSuccessAsync(Func<T, Task> action)
    {
        if (IsSuccess)
        {
            await action(Value);
        }

        return this;
    }

    public async Task<Result<T>> OnFailureAsync(Func<Error, Task> action)
    {
        if (IsFailure)
        {
            await action(Error);
        }

        return this;
    }

    public Result<U> Map<U>(Func<T, U> map)
    {
        return IsSuccess ? Result.Success(map(Value)) : Result.Failure<U>(Error);
    }

    public Result<U> Bind<U>(Func<T, Result<U>> next)
    {
        return IsSuccess ? next(Value) : Result.Failure<U>(Error);
    }

    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess)
        {
            action(Value);
        }

        return this;
    }

    public Result<T> TapError(Action<Error> action)
    {
        if (IsFailure)
        {
            action(Error);
        }

        return this;
    }

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }

    public Result<U> Then<U>(Func<T, U> onSuccess)
    {
        return IsSuccess ? Result.Success(onSuccess(Value)) : Result.Failure<U>(Error);
    }

    public new Result<T> Finally(Action action)
    {
        action();
        return this;
    }

    public static implicit operator Result<T>(T value) => Success(value);

    public static Result<T> FromFailure(Error error) => Failure<T>(error);
}
