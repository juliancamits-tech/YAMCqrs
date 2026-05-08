namespace YAMCqrs.Core;

public class Result<TResult>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public TResult? Value { get; }
    public string Error { get; }

    private Result(bool success, TResult? value, string error)
    {
        IsSuccess = success;
        Value = value;
        Error = error;
    }

    public static Result<TResult> Success(TResult value) => new(true, value, string.Empty);
    public static Result<TResult> Ok(TResult value) => Success(value);

    public static Result<TResult> Failure(string error) => new(false, default, error);
    public static Result<TResult> Fail(string error) => Failure(error);

    public static implicit operator Result<TResult>(TResult value) => Success(value);
    public static implicit operator Result<TResult>(string error) => Failure(error);
}