namespace YAMCqrs.Core;

public class Result<TResult>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public TResult? Value { get; }
    public string Error { get; }

    internal Result(bool success, TResult? value, string error)
    {
        IsSuccess = success;
        Value = value;
        Error = error;
    }
}

public static class Result
{
    // Métodos de Fábrica Estándar (Aprovechan inferencia de tipos)
    public static Result<TResult> Success<TResult>(TResult value) => new(true, value, string.Empty);
    public static Result<TResult> Ok<TResult>(TResult value) => Success(value);
    public static Result<TResult> Failure<TResult>(string error) => new(false, default, error);
    public static Result<TResult> Fail<TResult>(string error) => Failure<TResult>(error);

    // Alternativas con nombre para los operadores implícitos (Exigidos por CA2225)
    public static Result<TResult> FromTResult<TResult>(TResult value) => Success(value);
    public static Result<TResult> ToResult<TResult>(TResult value) => Success(value);

    // Nota que aquí sí requerimos el parámetro genérico <TResult> porque recibimos un string
    public static Result<TResult> FromString<TResult>(string error) => Failure<TResult>(error);
}
