# Module Communication

## Purpose

This document describes how business modules communicate with each other while preserving module boundaries.

JobWize follows a modular monolith architecture where each module owns its business logic, persistence, and public contracts.

Modules never reference another module's implementation directly.

Instead, communication occurs through well-defined contracts and a shared dispatcher abstraction.

---

# Communication Principles

The communication architecture follows these principles.

-   Modules own their data.
-   Modules expose contracts, never implementations.
-   Commands never cross module boundaries.
-   Queries may retrieve data from another module.
-   Integration events notify modules that something has happened.
-   Communication infrastructure is hidden behind a shared dispatcher abstraction.

---

# Communication Types

Modules communicate in three different ways.

| Type                     | Purpose                                          | Transport         |
| ------------------------ | ------------------------------------------------ | ----------------- |
| Local Commands / Queries | Execute business logic inside the current module | MediatR           |
| Module Queries           | Read information from another module             | Module Dispatcher |
| Integration Events       | Notify other modules about business events       | Message Broker    |

```mermaid
flowchart TD

    A["Application Handler"]

    D["IDispatcher"]

    M["MediatR"]

    Q["Module Dispatcher"]

    B["Message Broker"]

    A --> D

    D --> M
    D --> Q
    D --> B

    classDef application fill:#dcfce7,stroke:#16a34a,color:#000;
    classDef infrastructure fill:#dbeafe,stroke:#2563eb,color:#000;

    class A application;
    class D,M,Q,B infrastructure;
```

---

# Dispatcher

The Application layer never communicates directly with MediatR, the message broker, or another module.

Instead, every feature depends on a shared dispatcher abstraction.

```csharp
public interface IDispatcher
{
    Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);

    Task<TResponse> SendAsync<TResponse>(
        IModuleQuery<TResponse> query,
        CancellationToken cancellationToken = default);

    Task PublishAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}
```

The dispatcher hides the communication mechanism from the business logic.

This allows the communication infrastructure to evolve without affecting application features.

---

# Local Communication

Commands and Queries executed inside the current module are processed through MediatR.

```mermaid
flowchart LR

    Endpoint --> Command

    Command --> Dispatcher

    Dispatcher --> MediatR

    MediatR --> Handler

    classDef application fill:#dcfce7,stroke:#16a34a,color:#000;

    class Endpoint,Command,Dispatcher,MediatR,Handler application;
```

The Application layer remains unaware of MediatR.

---

# Module Queries

Modules frequently require information owned by another module.

For example:

```text
Applications Module

↓

GetUserById

↓

Identity Module
```

Rather than accessing another module's database, a synchronous Module Query is executed.

```mermaid
flowchart LR

    A["Applications"]

    D["IDispatcher"]

    Q["GetUserById.ModuleQuery"]

    I["Identity"]

    A --> D

    D --> Q

    Q --> I

    classDef module fill:#dcfce7,stroke:#16a34a,color:#000;
    classDef infrastructure fill:#dbeafe,stroke:#2563eb,color:#000;

    class A,I module;
    class D,Q infrastructure;
```

Only contracts are shared between modules.

Module implementations remain encapsulated.

---

# Public Queries vs Module Queries

A feature may be exposed through both the public HTTP API and module-to-module communication.

Although they may represent the same business capability, they are considered different application entry points.

For example:

```text
GetUserById.cs

GetUserById
├── Query
├── ModuleQuery
├── QueryHandler
├── ModuleQueryHandler
└── Endpoint
```

This allows both use cases to evolve independently.

Typical differences include:

-   Authorization rules.
-   Visibility of soft-deleted entities.
-   Returned data.
-   Business validations.

The small amount of duplicated code is intentional and keeps architectural boundaries explicit.

---

# Integration Events

Integration Events represent business events that have occurred within a module.

They serve two purposes:

-   Notify handlers inside the current module to complete local business orchestration.
-   Notify other modules that a business event has occurred.

Integration Events are never used to request information from another module.

Example:

```text
UserCreated

↓

Identity Module
├── Generate Refresh Token
├── Send Welcome Email
└── Audit Logging

↓

Notifications
Applications
Companies
```

Application features publish integration events through the dispatcher.

The feature remains completely unaware of how notifications are processed or delivered.

```mermaid
flowchart LR

    Handler --> Dispatcher

    Dispatcher --> MediatR

    MediatR --> LocalHandlers["Local Notification Handlers"]

    LocalHandlers --> Outbox

    Outbox --> MessageBroker["Message Broker"]

    MessageBroker --> Notifications

    MessageBroker --> Applications

    MessageBroker --> Companies

    classDef application fill:#dcfce7,stroke:#16a34a,color:#000;
    classDef infrastructure fill:#dbeafe,stroke:#2563eb,color:#000;

    class Handler,LocalHandlers,Notifications,Applications,Companies application;
    class Dispatcher,MediatR,Outbox,MessageBroker infrastructure;
```

---

# Event Publishing

Application features publish an integration event only once.

```csharp
await dispatcher.PublishAsync(
    new UserCreated(...),
    cancellationToken);
```

The dispatcher is responsible for the complete publishing workflow:

1. Publishing the event inside the current module using MediatR.
2. Waiting for all local notification handlers (including nested notifications) to complete.
3. Recording the event in the Outbox.
4. Allowing the Outbox Processor to publish the event to the message broker after the surrounding transaction has been committed.

This guarantees that local business orchestration always completes before other modules are notified.

Application features remain completely unaware of these implementation details.

> **Note**
>
> The dispatcher does **not** publish directly to the message broker. Its responsibility is to coordinate the publishing workflow. Reliable delivery to other modules is handled asynchronously by the Outbox Processor, ensuring events are only published after a successful transaction commit.

---

# Choosing the Communication Pattern

```mermaid
flowchart TD

    A["Need another module?"]

    B{"Need data?"}

    C["Module Query"]

    D{"Business event occurred?"}

    E["Publish Integration Event"]

    A --> B

    B -->|Yes| C

    B -->|No| D

    D -->|Yes| E

    classDef decision fill:#fde68a,stroke:#d97706,color:#000;
    classDef action fill:#dcfce7,stroke:#16a34a,color:#000;

    class B,D decision;
    class C,E action;
```

---

# Design Principles

The communication architecture follows these rules.

-   Modules never reference another module's implementation.
-   Modules never access another module's database.
-   Commands never cross module boundaries.
-   Queries retrieve information synchronously.
-   Integration events notify other modules asynchronously.
-   Application features communicate only through the shared dispatcher.
-   Communication infrastructure remains hidden from business logic.
-   Public and module queries may evolve independently when their responsibilities differ.
