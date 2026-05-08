# 11. Tests

Date: 2026-05-01

## Status

Accepted

## Context

We need to define a consistent strategy for organizing test projects across our solution portfolio. Different project types (core libraries, analyzers, source generators) have different testing requirements, and we need to balance between project isolation and practical maintenance.

## Decision

We will adopt a tiered approach to test project organization:

### Standard Rule: One Test Project Per Project

Each main project should have its own dedicated test project. This provides:
- Clear ownership and boundaries
- Independent build/test cycles
- Easier CI/CD configuration

### Exception: Analyzer and Source Generator Projects

For projects that contain Analyzer or Source Generator components, we allow grouping tests into a single test project with folder-based division:

```
src/
  Project/
    Project.csproj              (Core library)
    Project.SourceGenerator.csproj
    Project.Analyzer.csproj
tests/
  Project.Tests/
    Project.Tests.csproj
    Core/                       (Tests for Project.csproj)
    SourceGenerator/            (Tests for Project.SourceGenerator.csproj)
    Analyzer/                   (Tests for Project.Analyzer.csproj)
```

### Rationale for the Exception

- **Shared infrastructure**: Both analyzers and source generators require similar test utilities (Roslyn test fixtures, compilation verification)
- **Small scope**: These project types typically have fewer tests than core libraries
- **Maintenance burden**: Maintaining separate test projects for small components adds overhead without significant benefit

## Consequences

### Positives

- **Consistency**: Clear rule for most projects reduces decision fatigue
- **Flexibility**: Exception clause handles special cases elegantly
- **Discoverability**: Folder-based organization makes it clear which tests belong to which component
- **CI/CD friendly**: Standard projects can have independent test pipelines

### Negatives

- **Inconsistent structure**: Some solutions will have separate test projects, others will have grouped tests
- **Potential test pollution**: Shared test projects could have unintended dependencies between component tests

### Migration Path

Existing solutions should migrate to this pattern gradually. New projects should follow this convention from the start.

