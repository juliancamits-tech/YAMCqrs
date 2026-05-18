# 9. Commands and Queries

Date: 2026-04-30

## Status

Accepted

## Context

In a CQRS with Mediator workflow, we need to define how the workflow entry points will be structured to be "dispatched" through the mediator pipeline.

## Decision

We will use `ICommand` and `IQuery` interfaces to define the workflow entry points, with corresponding `ICommandHandler` and `IQueryHandler` interfaces for the logic implementation.

### Why Both?

Using separate interfaces for Commands and Queries allows us to apply different logic at various points in the lifecycle:

- **Validation rules:** "Queries should never alter data" or "Commands should always be transactional"
- **Event handling:** "You can't create an event for a Query because Query is just read data"
- **Pipeline behaviors:** Different middleware, logging, or authorization rules for each type
- **Semantic clarity:** The codebase clearly distinguishes between read and write operations

## Consequences

### Positives

- **Clear separation:** Enables different logic applied throughout the lifecycle for Commands vs Queries
- **Flexibility:** Allows enforcing different rules and behaviors for each operation type
- **Self-documenting code:** The interface types make the intent explicit
- **Extensibility:** Easy to add new behaviors specific to Commands or Queries

### Negatives

- **Potential code duplication:** Some common patterns may need to be duplicated to understand both interfaces
- **Additional abstraction layers:** Developers need to understand both interfaces and their handlers