# 7. BackgroundService

Date: 2026-04-29

## Status

Accepted

## Context

In C#, BackgroundService is the recommended approach for running continuous or long-running tasks in the background, typically using a main loop that operates as long as the application is running. Properly structuring background services is essential for reliability, maintainability, and observability, especially in systems that process domain events or batches of work.

## Decision

As described in ADR 0006 - Domain Event, events are stored for later processing. To process these events in their own scope, we require a robust and standardized implementation of BackgroundService. The goal is to provide a clean, step-based template that can be easily adopted and extended by developers.

The standardized BackgroundService implementation will follow these steps:

- Startup Check: Determine if the service should run at application startup (e.g., using feature flags). If not, terminate the service.
- Initial Sleep (if needed): Optionally delay processing if the application has just restarted.
- Batch Processing Loop:
    - Batch Execution Check: Decide whether to execute the batch (e.g., skip if a required external API is unavailable, logging the issue once to avoid excessive logs and wasted resources).
    - Create Batch Audit: Record audit information for the batch.
    - Pre-Batch Preparation: Perform any setup required before processing.
    - Retrieve Batch: Fetch the items to be processed.
    - Process Items: Process each item in the batch.
    - Post-Processing Cleanup: Clean up resources or perform any finalization steps.
    - Save Batch Audit: Persist audit information for the batch.
- Finalize Task: Cleanly shut down the service when required.

Developer Configuration Options:

- Instance Count: Specify the number of service instances to process items in parallel.

- Sleep Time Mode:
    - Standard: Sleep for a fixed, defined interval between batches.
    - Delta: Sleep for the difference between the defined interval and the actual batch processing time (e.g., if the interval is 5 minutes and the batch takes 4 minutes, sleep for 1 minute; if the batch takes 5 minutes or more, start the next batch immediately).
- Sleep Time Between Batches: Configure the interval between batch executions.

To ensure reliability and observability, batch processing state and audit information will be stored in a persistent storage mechanism. This enables safe recovery after application restarts and provides data for auditing and health checks.

## Consequences

### Positives

- Provides a standardized, maintainable template for background processing.
- Improves reliability and observability through persistent audit and state storage.
- Supports flexible configuration for parallelism and scheduling.
- Reduces the risk of resource waste and excessive logging during external system outages.
- Facilitates health checks and monitoring of background services.

### Negatives

- Increases implementation complexity due to additional configuration and storage requirements.
- Requires ongoing maintenance of the standardized template and supporting infrastructure.