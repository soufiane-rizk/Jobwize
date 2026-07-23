# ADR-003 - Module Communication Strategy

## Status

Accepted

> **Related ADRs**
>
> -   ADR-010 – Introduce a Custom Module Runtime
> -   ADR-011 – Introduce Runtime Execution Models

---

# Context

JobWize is composed of independent business modules.

Although modules collaborate to fulfill business requirements, they must remain loosely coupled and communicate through well-defined contracts rather than direct implementation dependencies.

Different communication scenarios have different requirements.

Some interactions execute business logic, others retrieve data, while others simply notify other modules that something has happened.

While the communication mechanisms remain stable, the runtime is responsible for determining how requests and notifications are executed.

The execution strategy itself is documented separately in ADR-011.

---

# Decision

JobWize adopts three distinct communication mechanisms, each serving a specific architectural purpose.

```mermaid
flowchart LR

    Feature["Application Feature"]

    Feature --> Dispatcher["IDispatcher"]

    Dispatcher --> Send["SendAsync()"]
    Dispatcher --> Query["SendModuleQueryAsync()"]
    Dispatcher --> Publish["PublishAsync()"]

    Send --> Runtime["Runtime"]

    Query --> ModuleDispatcher["Module Dispatcher"]

    Publish --> Runtime

    Runtime --> Execution["Execution Model"]

    Execution --> Local["Notification Handlers"]

    classDef application fill:#dcfce7,stroke:#16a34a,color:#000,stroke-width:2px;
    classDef infrastructure fill:#dbeafe,stroke:#2563eb,color:#000,stroke-width:2px;
    classDef runtime fill:#fde68a,stroke:#ca8a04,color:#000,stroke-width:2px;

    class Feature application;
    class Dispatcher,ModuleDispatcher infrastructure;
    class Runtime,Execution,Local runtime;
```

Each dispatcher method represents a different communication pattern.

| Method                   | Purpose                                              |
| ------------------------ | ---------------------------------------------------- |
| `SendAsync()`            | Execute application logic inside the current module. |
| `SendModuleQueryAsync()` | Retrieve authoritative data from another module.     |
| `PublishAsync()`         | Notify other modules that something has happened.    |

Each mechanism exists because it represents a different architectural responsibility.

---

## Relationship with the Runtime

This ADR defines the communication mechanisms available to application features.

It does not define how those mechanisms are executed.

Request execution is provided by the Runtime introduced in ADR-010.

Notification execution is delegated to Runtime Execution Models introduced in ADR-011.

This separation allows communication semantics to remain stable while execution behavior evolves independently.

# Consequences

## Positive

-   Communication intent is explicit in the code.
-   Module boundaries remain well defined.
-   Synchronous and asynchronous communication are clearly separated.
-   Different communication patterns can evolve independently.
-   Future execution models do not affect application features.

## Trade-offs

Developers must understand when each communication mechanism is appropriate.

Although introducing multiple communication patterns increases the number of abstractions, each abstraction has a single, well-defined responsibility.

---

# Alternatives Considered

## Single Dispatcher Method

A generic dispatcher API such as:

```csharp
await dispatcher.DispatchAsync(...);
```

could execute every type of operation.

### Advantages

-   Smaller public API.
-   One generic entry point.

### Reasons Not Chosen

Different communication patterns have different semantics.

Executing business logic, retrieving data from another module, and publishing an event are fundamentally different operations.

Using dedicated methods makes the architectural intent immediately visible in the code.

---

## Single Request Execution Engine

Another option would have been to route every communication pattern through the same execution mechanism.

### Advantages

-   One communication library.
-   Consistent programming model.

### Reasons Not Chosen

Routing every communication pattern through a single execution mechanism would couple communication semantics to one runtime implementation.

JobWize intentionally separates communication intent from execution strategy.

Communication patterns remain stable while the Runtime determines how requests and notifications are executed.

---

## HTTP for Cross-Module Communication

Modules could communicate through HTTP, even while executing within the same application process.

### Advantages

-   Consistent with a future microservice architecture.
-   Well-understood communication model.

### Reasons Not Chosen

This introduces unnecessary serialization, networking, and infrastructure overhead for modules executing within the same process.

The architecture preserves the abstraction while allowing the implementation to evolve if modules are extracted into independent services.

---

## Integration Events for Every Interaction

Another alternative is to communicate exclusively through asynchronous events.

### Advantages

-   Loose coupling.
-   Fully event-driven architecture.

### Reasons Not Chosen

Integration events communicate that something has already happened.

They are not intended for request-response interactions or retrieving authoritative data.

Using events for synchronous communication would unnecessarily complicate workflows and reduce clarity.

---

# Rationale

Different communication scenarios require different architectural solutions.

By exposing dedicated methods through a single dispatcher abstraction, JobWize keeps communication intent explicit while allowing execution strategies to evolve independently.

The Dispatcher defines how modules communicate.

The Runtime determines how requests are executed.

Execution Models determine how published notifications are orchestrated.

Each responsibility evolves independently without affecting application features.
