# 8. Result Object

Date: 2026-04-30

## Status

Accepted

## Context

We need a standardized, type-safe, and readable way to communicate the success or failure of an operation in C#, while maintaining strong typing throughout the codebase.

Traditionally, the project has used two approaches to handle scenarios where a method cannot fulfill its function (e.g., user not found or validation failed):

- **Returning null:** This is ambiguous and often leads to NullReferenceException errors.
- **Throwing exceptions:** Exceptions are expensive in terms of performance and should be reserved for truly unexpected situations (network errors, database down), not for flow control logic.

## Decision

We will implement the Result Object pattern using a generic `Result<T>` class with private constructors and static factory methods.

### Technical Guidelines

1. **Encapsulation:** The constructor will be private to prevent inconsistent states.
2. **Generics Usage:** Using `<T>` avoids the use of `object` and ensures the return value is of the expected type without needing casting.
3. **Implicit Operators:** Implicit conversion operators will be included to simplify return syntax in business logic methods.

### Reference Implementation

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public string Error { get; }

    private Result(bool success, T value, string error)
    {
        IsSuccess = success;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, null);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(string error) => Failure(error);
}
```

## Consequences

### Positives

- **Type Safety:** The compiler guarantees the return type is correct, eliminating runtime errors from invalid conversions.
- **Expressiveness:** The code is self-documenting; when seeing a method signature `Result<User>`, the developer immediately knows the method can fail and must handle that case.
- **Performance:** Eliminates the overhead of capturing stack traces by not using exceptions for common logic.
- **Clean Syntax:** Thanks to implicit operators, the code remains concise (e.g., `return "Error message";`).

### Negatives

- **Verbosity in Consumption:** The caller must now always verify `if (result.IsSuccess)` before accessing the value, adding a couple of extra lines of code.
- **Learning Curve:** Developers accustomed to traditional try-catch blocks may need time to adapt to this functional paradigm.