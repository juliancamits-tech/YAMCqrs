# 5. Use of Roslyn

Date: 2026-04-29

## Status

Accepted

## Context

C# includes a powerful feature called Roslyn, which provides a compiler-as-a-service API. Roslyn enables developers to perform tasks during compilation, such as generating additional code (source generation) or analyzing code for potential issues (analyzers). Leveraging Roslyn can help improve code quality, enforce best practices, and reduce runtime overhead by shifting certain tasks to compile time.

## Decision

Building on the principles outlined in ADR 0004 - Use of Reflection, we will adopt Roslyn for the following purposes:

-Source Generation:
Roslyn will be used to generate code that eliminates the need for runtime reflection. This approach ensures that reflection-based operations are replaced with explicit, compile-time-generated code, improving performance and maintainability.

-Custom Analyzers:
We will create custom Roslyn analyzers to enforce best practices within the codebase. These analyzers will help identify and prevent the use of discouraged patterns, such as improper use of reflection or other uncommon practices that could negatively impact the application.

## Consequences

### Positives

- Improved Performance: By replacing runtime reflection with compile-time code generation, we reduce runtime overhead and improve application performance.
- Enforced Best Practices: Custom analyzers ensure consistent adherence to coding standards and library usage guidelines.
- Enhanced Code Quality: Shifting logic to compile time reduces the likelihood of runtime errors and improves code clarity.

### Negatives

- Increased Development Effort: Implementing and maintaining source generators and custom analyzers requires additional time and expertise.