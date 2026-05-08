# 6. Domain Events

Date: 2026-04-29

## Status

Accepted

## Context

Many systems use the concept of domain events to trigger actions after certain business events occur and are completed. Domain events help decouple business logic and enable extensibility, but their implementation and evolution can impact integration with other systems.

## Decision

We will adopt a flexible approach to domain events, recognizing that today's domain event may become tomorrow's integration event. To support this, the following rules and architecture will be established:

- We will create a specialized extension for Servibus Events, where "InMemory events" are the preferred mechanism for implementing domain events. This extension will be technology-agnostic; each messaging technology (e.g., Kafka, RabbitMQ, Azure Service Bus) will have its own extension built on top of the Servibus extension.
- Developers must explicitly call an interface to record the intention to publish an event.
- Events must inherit from a specific base class, which will route them to the appropriate bus.
- Event intentions will only be persisted if the current scope completes successfully (e.g., after a successful transaction).
- Events will be dispatched in a new, independent scope to ensure isolation and reliability.
- Extensions should be able to both send and receive messages, supporting bidirectional communication.
- Events will be stored in a configurable storage mechanism to enable processing in different scopes. By default, storage will be in-memory, but for production environments, a persistent store (such as a database) is recommended.

## Consequences

### Positives

- Promotes a seamless transition from domain events to integration events by simply changing the event's base class.
- Provides a unified interface for event dispatching, regardless of the underlying provider.
- Supports multiple bus providers within the same application, minimizing changes required by developers to switch or add providers.
- Enables listening to multiple events from multiple providers with minimal developer effort.
- Executing events in separate scopes allows for auditing and tracking event execution

### Negatives

- Additional extensions are required to support each bus provider.
- Additional extensions are needed to support each storage for save the Events
- This new way can cause rejection for dev-users