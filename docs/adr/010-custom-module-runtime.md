# ADR-010 – Introduce a Custom Module Runtime

## Status

**Superseded by ADR-011**

---

## Context

JobWize follows a **modular monolith** architecture where each module represents an autonomous business capability with clear ownership of its domain, application, infrastructure, and contracts.

While evaluating the request execution pipeline, MediatR was initially adopted to dispatch commands, queries, and notifications. Although MediatR is an excellent library for layered monoliths and microservice-based applications, its execution model assumes a **single global mediator** responsible for discovering and dispatching all handlers registered within the application.

This assumption conflicts with JobWize's architectural goals.

Modules in JobWize are intended to behave as independent runtime units. Each module should own its handlers, expose only explicit contracts, and remain isolated from the internal implementation details of other modules.

As the architecture evolved, several limitations became apparent:

-   Handler discovery was global rather than module-scoped.
-   Notifications were dispatched to every registered handler without respecting module ownership.
-   The runtime had no notion of which module owned a particular request.
-   Supporting future out-of-process module communication would require bypassing MediatR's execution model.
-   Architectural boundaries could not be enforced by the runtime itself.

---

## Requirements

The request execution runtime must satisfy the following requirements:

-   Every request must belong to exactly one module.
-   Every request must have exactly one handler.
-   Handler ownership must be validated during application startup.
-   Modules must own their runtime metadata independently.
-   Dispatching logic must be aware of module boundaries.
-   Cross-module communication must remain explicit.
-   The runtime must remain compatible with future pipeline behaviors.
-   The runtime must support future transport implementations (gRPC, messaging, etc.) without changing application code.

---

## Alternatives Considered

### Continue using MediatR

Rejected.

Although functional, MediatR's global handler registry does not align with module ownership and isolation requirements.

---

### Multiple MediatR instances

Rejected.

Managing multiple mediator instances introduces unnecessary complexity while still relying on MediatR's assumptions regarding handler discovery and notification dispatching.

---

### Child dependency injection containers

Rejected.

While technically possible, child containers significantly complicate dependency injection and service lifetimes for little architectural benefit.

---

### Keyed dependency injection

Rejected.

Keyed registrations improve service resolution but do not solve the fundamental issue that request dispatching remains globally coordinated.

---

### Custom module runtime

Accepted.

A dedicated runtime allows module ownership, validation, dispatching, and future extensibility to be implemented explicitly according to the needs of the architecture.

---

## Decision

JobWize introduces a custom runtime responsible for request discovery, validation, registration, and execution.

The runtime is composed of the following components:

-   **HandlerScanner** – Discovers requests and handlers within a module assembly.
-   **ModuleValidator** – Validates runtime rules during application startup.
-   **ModuleDescriptor** – Describes the runtime metadata of a module.
-   **HandlerCatalog** – Stores handler metadata for a single module.
-   **ModuleRuntime** – Executes requests belonging to a module.
-   **ModuleRegistry** – Resolves the owning module for each request.
-   **Dispatcher** – Entry point used by the application layer.
-   **HandlerInvoker** – Executes strongly-typed handlers without reflection during request execution.

Application startup now follows this sequence:

1. Configure module services.
2. Scan the module assembly.
3. Validate discovered requests and handlers.
4. Register handlers into the dependency injection container.
5. Create the module runtime.
6. Register the module runtime inside the global module registry.

At execution time the request flow becomes:

```text
Endpoint
    │
    ▼
Dispatcher
    │
    ▼
ModuleRegistry
    │
    ▼
ModuleRuntime
    │
    ▼
HandlerCatalog
    │
    ▼
HandlerInvoker
    │
    ▼
Handler
```

Each request is therefore executed by the runtime of the module that owns it.

---

## Consequences

### Positive

-   Strong enforcement of module boundaries.
-   Startup validation of handler ownership.
-   Complete control over request dispatching.
-   Clear separation between intra-module execution and inter-module communication.
-   Foundation for future transport implementations.
-   Removal of an external dependency from the application's core execution model.

### Negative

-   Additional framework code must be maintained by the project.
-   Runtime infrastructure becomes part of the application's architecture and therefore requires testing and documentation.
