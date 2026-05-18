# 1. Dependencies with External Packages

Date: 2026-04-20

## Status

Accepted

## Context

When developing the `YACqrs` library, we need to decide how to handle third-party dependencies (databases, event buses, message brokers, etc.) that add specific functionality to the CQRS framework.

### Issues to Consider

1. **Package Weight:** If we include all dependencies in the main package, users will download libraries they don't need
2. **Version Conflicts:** Different projects may require different versions of the same dependencies
3. **Flexibility:** Users should be able to choose which technologies to use (MongoDB, SQL Server, Kafka, RabbitMQ, etc.)
4. **Maintainability:** Updates to external dependencies should not force updates to the CQRS core

### Examples of External Dependencies

- **Databases:** MongoDB.Driver, Npgsql, Microsoft.EntityFrameworkCore
- **Message Brokers:** Confluent.Kafka, RabbitMQ.Client, Azure.Messaging.ServiceBus
- **Serialization:** Newtonsoft.Json (if a specific version is required)
- **Caching:** StackExchange.Redis

## Decision

The **main package** must be **free of external NuGet dependencies**, with the following allowed exceptions:

### Allowed Dependencies in the Main Package

Only Microsoft packages related to the .NET Core ecosystem:
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Hosting.Abstractions`
- `System.Text.Json` (part of the framework)

### Extension Architecture

To add features that require external dependencies, **separate extension packages** must be created:

#### Example of Extensions

```
YACqrs (core)
├── No heavy external dependencies
└── Defines interfaces

YACqrs.ServiceBus (extension)
├── Depends on → YACqrs
└── Implements abstraction to publish Integration Events to message brokers

YACqrs.Kafka (extension)
├── Depends on → YACqrs
├── Depends on → YACqrs.ServiceBus
└── Implements specific Kafka integration with CQRS
```

### Location of Extension Packages

Extension packages **can be in the same repository** but **MUST NOT be part of the main csproj**. Each one is an independent project that:
1. Has its own `.csproj` file
2. Is packaged as an independent NuGet
3. Can be updated without affecting the core

## Consequences

### Positives

1. **Lightweight Main Package:**
   - Users only download what they need
   - Fast installation for simple projects
   - No unnecessary dependency conflicts

2. **Technological Flexibility:**
   - Users can choose MongoDB, SQL Server, PostgreSQL, etc.
   - Easy to add support for new technologies
   - Not coupled to a specific provider

3. **Better Version Management:**
   - Hotfixes focused on the technology that needs them

4. **Simpler Development:**
   - Faster testing without needing to spin up external services
   - Easier onboarding for new developers

5. **Compliance with SOLID Principles:**
   - **Open/Closed:** Extensible without modifying the core
   - **Dependency Inversion:** Everything depends on abstractions
   - **Single Responsibility:** Each package has a clear responsibility

### Negatives

1. **Multiple Packages:**
   - Users must install multiple NuGet packages
   - More complexity in the initial installation
   - **Mitigation:** Clear documentation with installation examples

2. **Version Synchronization:**
   - Need to coordinate compatible versions between packages
   - Breaking changes in the core affect all extensions
   - **Mitigation:** Strict semantic versioning and integration testing

3. **Code Fragmentation:**
   - Related code is in different projects
   - PRs may require changes in multiple projects
   - **Mitigation:** Monorepo (all projects in the same repository)
