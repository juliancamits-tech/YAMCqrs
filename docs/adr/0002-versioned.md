# 2. Code Versioning Policy

Date: 2026-04-21

## Status

Accepted

## Context

We need to define a versioning strategy for our NuGet packages to ensure consistency, compatibility, and ease of maintenance across the project.

## Decision

All packages in the project will share a unified version number. This approach simplifies dependency management and avoids version mismatches between related packages. Maintaining individual versions for each package would add significant complexity and overhead.

We will use the versioning scheme X.Y.Z, where:

- **X**: Indicates the target .NET version. This is incremented when support for a new major .NET version is added or when breaking changes are introduced that require consumers to update their code.
- **Y**: Incremented when new public features are added or when there are breaking changes in the public API (e.g., a method signature changes or new required parameters are introduced).
- **Z**: Incremented for bug fixes and internal changes that do not affect the public API or developer experience.

### Examples

- If Microsoft releases .NET 12 and we update our packages to support it, we increment **X**.
- If we add a new public method or change an existing method in a way that affects consumers, we increment **Y**.
- If we fix a bug in a private function (e.g., change a conditional from '>' to '>='), we increment **Z**.

## Rationale

Using a unified versioning scheme ensures that all packages remain compatible and reduces confusion for users. It also streamlines the release process and documentation.

## Consequences

### Positives

- Simplifies dependency management for users and maintainers.
- Reduces the risk of version conflicts between packages.
- Easier to document and communicate changes across the ecosystem.

### Negatives

- Some packages may be released with a new version even if they have not changed.
- Increases CI/CD workload, as all packages are rebuilt and published together.

#### Mitigations

- Automate the release process to minimize manual effort.
- Clearly document the versioning policy for contributors and users.