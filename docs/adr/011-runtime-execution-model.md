# ADR-011 – Introduce Runtime Execution Models

## Status

**Accepted**

---

## Context

ADR-010 introduced a custom runtime to replace MediatR and enforce module ownership during request execution.

The initial runtime successfully addressed handler discovery, module validation, and request dispatching. However, it assumed a single execution strategy in which notifications were always processed immediately within the current process.

As the architecture evolved, notification execution itself became an architectural concern rather than an implementation detail.

Future versions of JobWize may execute notifications using different mechanisms depending on deployment requirements.

Examples include:

-   In-process execution.
-   Distributed messaging.
-   Hybrid execution models.
-   External workflow orchestration.

These execution strategies should not require changes to application handlers or business logic.

Application code should publish a notification once without knowing how, where, or when that notification will be processed.

---

## Requirements

The runtime must satisfy the following requirements:

-   Notification execution must be independent from application code.
-   Application handlers must publish notifications only once.
-   Notification execution strategy must be configurable.
-   Different execution strategies must be interchangeable.
-   The runtime must remain transport-independent.
-   Cross-module communication must remain an infrastructure concern.
-   Execution behavior must evolve without modifying business features.
-   Future distributed execution must reuse the same application code.

---

## Alternatives Considered

### Hardcoded Runtime Execution

Rejected.

Embedding notification execution directly inside the runtime tightly couples the runtime to a single execution strategy and prevents future architectural evolution.

---

### Notification Dispatcher

Rejected.

Introducing a dedicated notification dispatcher separates notification publishing from request execution but still centralizes execution behavior inside a single implementation.

This approach does not provide sufficient flexibility for future execution models.

---

### Runtime Strategy Pattern

Rejected.

Using a traditional strategy pattern would improve flexibility but still exposes execution strategy as an implementation detail rather than an architectural concept.

The runtime requires explicit ownership over notification execution as part of the application's execution model.

---

### Runtime Execution Models

Accepted.

The runtime delegates notification execution to a dedicated Execution Model abstraction responsible for orchestrating notification handlers according to the configured execution strategy.

---

## Decision

JobWize introduces **Execution Models** as the mechanism responsible for notification execution.

The runtime is divided into two complementary responsibilities.

### Execution Pipelines

Execution Pipelines implement cross-cutting concerns surrounding request execution.

Typical responsibilities include:

-   Validation.
-   Transaction management.
-   Logging.
-   Metrics.
-   Authorization.

Execution Pipelines execute before and after the application handler.

---

### Execution Models

Execution Models determine how published notifications are processed.

Application handlers publish notifications without knowing the execution strategy.

The configured Execution Model is responsible for:

-   Receiving published notifications.
-   Coordinating notification execution.
-   Determining execution order.
-   Supporting nested notifications.
-   Managing execution strategy.

The current implementation provides:

-   **MonolithExecutionModel**

Future implementations may include:

-   **DistributedExecutionModel**
-   **MessageBrokerExecutionModel**
-   **HybridExecutionModel**

without requiring changes to application code.

---

## Execution Flow

```text
Endpoint
    │
    ▼
Dispatcher
    │
    ▼
Execution Pipeline
    │
    ▼
Handler
    │
    ├──────────────► Persistence
    │
    ▼
Publish Notification
    │
    ▼
Execution Model
    │
    ▼
Notification Handlers
```

The Application layer remains unaware of the configured execution model.

---

## Consequences

### Positive

-   Notification execution becomes configurable.
-   Application features remain independent from transport concerns.
-   The runtime supports multiple execution strategies.
-   Monolith and distributed architectures share the same application code.
-   Notification orchestration becomes a dedicated architectural responsibility.
-   New execution models can be introduced without modifying business features.

### Negative

-   The runtime becomes more sophisticated.
-   Multiple execution models require additional testing.
-   Notification orchestration becomes a core framework responsibility.

---

## Relationship to ADR-010

This ADR extends the runtime introduced in **ADR-010 – Introduce a Custom Module Runtime**.

ADR-010 established the custom runtime responsible for request discovery, handler resolution, and module ownership.

ADR-011 introduces the concept of **Execution Models**, allowing notification execution to evolve independently from the runtime itself while preserving a consistent programming model for the Application layer.

The runtime remains responsible for request execution, while Execution Models become responsible for notification orchestration.
