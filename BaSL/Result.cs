using System.Diagnostics.CodeAnalysis;

namespace BaSL;

public readonly record struct Result<TResult, TError>
{

    public static Result<TResult, TError> CreateSuccess(TResult result) => result;

    public static Result<TResult, TError> CreateError(TError error) => error;

    public static implicit operator Result<TResult, TError>(TResult result) => new(result, default, true);

    public static implicit operator Result<TResult, TError>(TError error) => new(default, error, false);

    private Result(TResult? value, TError? error, bool success)
    {
        Value = value;
        Error = error;
        Success = success;
    }

    public TResult? Value { get; }

    public TError? Error { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success { get; }

}
