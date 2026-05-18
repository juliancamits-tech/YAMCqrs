# 10. Pipeline Interceptors

Date: 2026-04-30

## Status

Accepted

## Context

In a CQRS with Mediator workflow, sometimes you want to add extra steps before or after the handler executes. We need to define how the developer can add logic in the middle of the pipeline in a consistent way.

## Decision

We are going to create the concept of "Interceptors" — they represent extra logic executed during the workflow. To make the implementation more implicit and clarify when they execute in the pipeline, the interceptor should have this structure:

### Interface Design

- **`ICommandInterceptor`** and **`IQueryInterceptor`** are the main interfaces
- An interceptor can be implemented to execute on:
  - All `ICommand` or `IQuery` types
  - A specific implementation of `ICommand` or `IQuery`
- Both interceptor interfaces should define:
  - `OnBeforeAsync` — executed before the handler (e.g., "Open a DB Transaction")
  - `OnAfterAsync` — executed after successful handler completion (e.g., "Commit transaction")
  - `OnErrorAsync` — executed when an error occurs (e.g., "Rollback transaction")

### Execution Order

The order of execution of interceptors is not based on "injection order" because this can cause problems when using NuGet packages that add their own interceptors. Instead, we will define a two-level ordering system:

- **Layer level:** Defines priority based on the application layer where the interceptor works
- **Order:** The value within the layer for fine-grained control

## Consequences

### Positives

- **Clear separation:** Interceptors provide a clean way to add cross-cutting concerns
- **Flexibility:** Can apply to all commands/queries or target specific ones
- **Explicit lifecycle:** `OnBeforeAsync`, `OnAfterAsync`, and `OnErrorAsync` make the execution point unambiguous
- **Predictable ordering:** The two-level ordering system prevents conflicts between packages

### Negatives

- **Additional abstraction layers:** Developers need to understand the interceptor pattern
- **Potential complexity:** Too many interceptors can make the flow harder to trace