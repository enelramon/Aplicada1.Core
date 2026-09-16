using Aplicada1.Core;

namespace Aplicada1.Core.Tests;

public class ResultTests
{
    [Fact]
    public void Result_OnSuccess_ShouldExecuteAction_WhenSuccessful()
    {
        var executed = false;

        var result = Result.Success();
        var returned = result.OnSuccess(() => executed = true);

        Assert.True(result.IsSuccess);
        Assert.True(executed);
        Assert.Same(result, returned);
    }

    [Fact]
    public void Result_OnFailure_ShouldExecuteAction_WhenFailure()
    {
        var executed = false;
        var error = new Error("E001", "Something went wrong");

        var result = Result.Failure(error);
        var returned = result.OnFailure(() => executed = true);

        Assert.True(result.IsFailure);
        Assert.True(executed);
        Assert.Same(result, returned);
    }

    [Fact]
    public void ResultOfT_OnSuccess_ShouldExecuteAction_WithValue()
    {
        var executed = false;

        var result = Result.Success("hello");
        var returned = result.OnSuccess(value =>
        {
            executed = true;
            Assert.Equal("hello", value);
        });

        Assert.True(executed);
        Assert.Same(result, returned);
    }

    [Fact]
    public void ResultOfT_OnFailure_ShouldExecuteAction_WithError()
    {
        var executed = false;
        var error = new Error("E002", "Validation failed");

        var result = Result.Failure<string>(error);
        var returned = result.OnFailure(err =>
        {
            executed = true;
            Assert.Equal(error, err);
        });

        Assert.True(executed);
        Assert.Same(result, returned);
    }

    [Fact]
    public async Task Result_OnSuccessAsync_ShouldExecuteAction_WhenSuccessful()
    {
        var executed = false;

        var result = Result.Success();
        var returned = await result.OnSuccessAsync(() =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        Assert.True(result.IsSuccess);
        Assert.True(executed);
        Assert.Same(result, returned);
    }

    [Fact]
    public async Task Result_OnFailureAsync_ShouldExecuteAction_WhenFailure()
    {
        var executed = false;
        var error = new Error("E003", "Async failure");

        var result = Result.Failure(error);
        var returned = await result.OnFailureAsync(() =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        Assert.True(result.IsFailure);
        Assert.True(executed);
        Assert.Same(result, returned);
    }

    [Fact]
    public async Task ResultOfT_OnSuccessAsync_ShouldExecuteAction_WithValue()
    {
        var executed = false;

        var result = Result.Success("async value");
        var returned = await result.OnSuccessAsync(value =>
        {
            executed = true;
            Assert.Equal("async value", value);
            return Task.CompletedTask;
        });

        Assert.True(executed);
        Assert.Same(result, returned);
    }

    [Fact]
    public async Task ResultOfT_OnFailureAsync_ShouldExecuteAction_WithError()
    {
        var executed = false;
        var error = new Error("E004", "Async validation failed");

        var result = Result.Failure<string>(error);
        var returned = await result.OnFailureAsync(err =>
        {
            executed = true;
            Assert.Equal(error, err);
            return Task.CompletedTask;
        });

        Assert.True(executed);
        Assert.Same(result, returned);
    }

    [Fact]
    public void ResultExtensions_ToResult_ShouldReturnSuccess_WhenConditionIsTrue()
    {
        var result = true.ToResult(new Error("E005", "Should not be used"));

        Assert.True(result.IsSuccess);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void ResultExtensions_ToResult_ShouldReturnFailure_WhenConditionIsFalse()
    {
        var error = new Error("E006", "Condition failed");
        var result = false.ToResult(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void ResultExtensions_ToSuccess_ShouldWrapValue()
    {
        var result = "value".ToSuccess();

        Assert.True(result.IsSuccess);
        Assert.Equal("value", result.Value);
    }

    [Fact]
    public void ResultExtensions_ToFailure_ShouldWrapError()
    {
        var error = new Error("E007", "Operation failed");
        var result = error.ToFailure<string>();

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Result_Bind_ShouldReturnNextResult_WhenSuccess()
    {
        var nextError = new Error("E008", "Second step failed");
        var result = Result.Success()
            .Bind(() => Result.Success())
            .Bind(() => Result.Failure(nextError));

        Assert.True(result.IsFailure);
        Assert.Equal(nextError, result.Error);
    }

    [Fact]
    public void ResultOfT_Map_ShouldTransformValue_WhenSuccess()
    {
        var result = Result.Success(2)
            .Map(x => x * 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.Value);
    }

    [Fact]
    public void ResultOfT_Bind_ShouldExecuteNestedResult_WhenSuccess()
    {
        var result = Result.Success(5)
            .Bind(value => Result.Success(value + 3));

        Assert.True(result.IsSuccess);
        Assert.Equal(8, result.Value);
    }

    [Fact]
    public void ResultOfT_Match_ShouldReturnEitherBranch()
    {
        var success = Result.Success("ok")
            .Match(value => value.ToUpperInvariant(), error => error.Description);

        var failure = Result.Failure<string>(new Error("E009", "bad"))
            .Match(value => value.ToUpperInvariant(), error => error.Description);

        Assert.Equal("OK", success);
        Assert.Equal("bad", failure);
    }

    [Fact]
    public void ResultOfT_Tap_ShouldExecuteAction_WhenSuccess()
    {
        var executed = false;
        var result = Result.Success("value")
            .Tap(value => executed = value == "value");

        Assert.True(result.IsSuccess);
        Assert.True(executed);
    }

    [Fact]
    public void ResultOfT_TapError_ShouldExecuteAction_WhenFailure()
    {
        var executed = false;
        var error = new Error("E010", "failure");
        var result = Result.Failure<string>(error)
            .TapError(err => executed = err == error);

        Assert.True(result.IsFailure);
        Assert.True(executed);
    }

    [Fact]
    public void Result_Finally_ShouldAlwaysExecuteAction()
    {
        var executed = false;

        var success = Result.Success().Finally(() => executed = true);
        var failure = Result.Failure(new Error("E011", "failure")).Finally(() => executed = true);

        Assert.True(success.IsSuccess);
        Assert.True(failure.IsFailure);
        Assert.True(executed);
    }

    [Fact]
    public void ResultOfT_Then_ShouldMapOrContinueSuccessFlow()
    {
        var result = Result.Success(7)
            .Then(value => value + 3)
            .Then(value => value * 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.Value);
    }

    [Fact]
    public void Result_Try_ShouldReturnFailure_WhenExceptionIsThrown()
    {
        var result = Result.Try(() => throw new InvalidOperationException("boom"));

        Assert.True(result.IsFailure);
        Assert.Equal("EXCEPTION", result.Error.Code);
        Assert.Equal("boom", result.Error.Description);
    }

    [Fact]
    public void ResultOfT_Try_ShouldReturnFailure_WhenValueFactoryThrows()
    {
        var result = Result.Try(() => int.Parse("not-a-number"));

        Assert.True(result.IsFailure);
        Assert.Equal("EXCEPTION", result.Error.Code);
        Assert.Contains("not-a-number", result.Error.Description);
    }

    [Fact]
    public async Task IServiceResult_ShouldExposeResultBasedContract()
    {
        var service = new FakeResultService();

        var saveResult = await service.Guardar("value");
        var getResult = await service.Buscar(1);
        var deleteResult = await service.Eliminar(1);
        var listResult = await service.GetList(x => x.Contains("value"));

        Assert.True(saveResult.IsSuccess);
        Assert.True(getResult.IsSuccess);
        Assert.True(deleteResult.IsSuccess);
        Assert.True(listResult.IsSuccess);
    }

    private sealed class FakeResultService : IServiceResult<string, int>
    {
        public Task<Result> Guardar(string entidad) => Task.FromResult(Result.Success());

        public Task<Result<string?>> Buscar(int id) => Task.FromResult(Result.Success<string?>("value"));

        public Task<Result> Eliminar(int id) => Task.FromResult(Result.Success());

        public Task<Result<List<string>>> GetList(System.Linq.Expressions.Expression<Func<string, bool>> criterio)
            => Task.FromResult(Result.Success(new List<string> { "value" }));
    }
}
