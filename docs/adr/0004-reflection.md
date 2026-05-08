# 4. Use of Reflection in C#

Date: 2026-04-25

## Status

Accepted

## Context

Reflection in C# provides powerful capabilities for inspecting and interacting with types at runtime. However, its use can significantly impact application performance, especially when compared to other operations that do not involve external resources (such as database or disk access). To ensure maintainable and performant code, it is important to define clear guidelines for when and how reflection should be used.

## Decision

Reflection usage is categorized into two levels: "metadata access" and "true reflection."

- Metadata access (e.g., retrieving a class name) is acceptable. These operations simply access information already available at runtime and do not incur significant overhead.

- True reflection (e.g., discovering methods dynamically, checking for inheritance or interface implementation at runtime) should be avoided or used only as a last resort. These operations can introduce hidden performance costs and make code harder to understand and maintain.

Where possible, alternatives such as source generators or explicit code should be preferred over runtime reflection.

## Consequences

### Positives

- Reduces the risk of hidden performance issues and helps maintain predictable application behavior.
- Makes code more explicit and easier for developers to understand and optimize.

### Negatives

- May require additional effort to implement source generators or write explicit code to replace certain reflection-based patterns