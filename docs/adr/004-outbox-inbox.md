# ADR-004 - Reliable Integration Event Delivery with Outbox and Inbox

## Status

Accepted

> **Related ADRs**
>
> -   ADR-010 – Introduce a Custom Module Runtime
> -   ADR-011 – Introduce an Execution Model for Request Processing

---

# Context

Business modules communicate through Notifications. The active Execution Model determines how those notifications are delivered. In the current modular monolith, notifications are processed in-process. When modules execute out-of-process, notifications may be delivered through reliable messaging using the Outbox and Inbox patterns.

The architecture must ensure that events are delivered reliably without creating inconsistencies between the application's database and the message broker.

The system must tolerate situations such as:

-   The database transaction succeeds but the message broker is temporarily unavailable.
-   A consumer receives the same message more than once.
-   A consumer fails while processing an event.
-   Temporary infrastructure failures during message publication or consumption.

Since the database and the message broker cannot participate in the same transaction, reliable event delivery requires a dedicated architectural approach.

---

# Decision

JobWize adopts the **Outbox** and **Inbox** patterns for all Integration Events.

Each business module owns:

-   An **Outbox** for reliable event publication.
-   An **Inbox** for reliable and idempotent event consumption.
-   An **Outbox Processor** responsible for publishing pending events.
-   An **Inbox Processor** responsible for processing received events.

Application features never publish events directly to the message broker.

Integration Events are first published through the application's notification pipeline. During request execution they are dispatched to in-process notification handlers before being persisted to the Outbox. Only after the transaction successfully commits are Outbox records processed asynchronously and delivered to the external message broker.

Instead, application features publish Integration Events through the IDispatcher. During command execution, the Execution Model dispatches the event to all registered in-process notification handlers. These handlers execute within the same transaction as the originating command.

After all notification handlers complete successfully, the Integration Event is persisted to the module's Outbox. Background processors later publish pending events to the external message broker and process received events through each module's Inbox.

---

## In-Process Notification Handling

Integration Events are processed twice during their lifecycle.

The first execution occurs **inside the originating module**, before the transaction commits.

When `Dispatcher.Publish()` is called, the Execution Model dispatches the notification to all registered in-process notification handlers. These handlers execute within the same transaction as the originating command.

Typical responsibilities include:

-   Updating local read models.
-   Coordinating multiple aggregates.
-   Scheduling additional business work.
-   Publishing additional notifications.
-   Preparing data that will later be consumed asynchronously.

Only after every in-process notification handler completes successfully are the notifications intended for other modules persisted to the Outbox.

This guarantees that all business logic inside the originating module completes atomically before asynchronous delivery begins.

```mermaid
flowchart LR

    subgraph Identity["Identity Module"]

        Feature["Application Feature"]

        Dispatcher["Dispatcher.Publish()"]

        Notifications["In-Process Notifications"]

        NotificationHandlers["Notification Handlers"]

        Outbox["Outbox"]

        OutboxProcessor["Outbox Processor"]

        Feature --> Dispatcher
        Dispatcher --> Notifications
        Notifications --> NotificationHandlers
        NotificationHandlers --> Outbox
        Outbox --> OutboxProcessor

    end

    Broker["RabbitMQ"]

    subgraph Companies["Companies Module"]

        Inbox["Inbox"]

        InboxProcessor["Inbox Processor"]

        Handler["Notification Handler"]

        Inbox --> InboxProcessor
        InboxProcessor --> Handler

    end

    OutboxProcessor --> Broker
    Broker --> Inbox

    classDef application fill:#dcfce7,stroke:#16a34a,color:#000,stroke-width:2px;
    classDef runtime fill:#ede9fe,stroke:#7c3aed,color:#000,stroke-width:2px;
    classDef infrastructure fill:#dbeafe,stroke:#2563eb,color:#000,stroke-width:2px;

    class Feature,NotificationHandlers,Handler application;
    class Dispatcher,Notifications runtime;
    class Outbox,Inbox,OutboxProcessor,InboxProcessor,Broker infrastructure;
```

| Pattern    | Responsibility                                                             |
| ---------- | -------------------------------------------------------------------------- |
| **Outbox** | Guarantees reliable publication of Integration Events.                     |
| **Inbox**  | Guarantees reliable, idempotent processing of received Integration Events. |

---

# Consequences

## Positive

-   Integration Events are never lost because of temporary broker failures.
-   Event consumption remains idempotent.
-   Transient failures can be retried automatically.
-   Failed messages can be isolated after retry exhaustion instead of being silently lost.
-   Poison messages do not repeatedly disrupt event processing, allowing the module to continue operating while the failed message awaits investigation.
-   Every module owns its complete event processing pipeline.

## Trade-offs

-   Additional database tables are required.
-   Background processing becomes part of every module.
-   Event delivery becomes eventually consistent.
-   The overall messaging infrastructure is more complex than direct publication.

These trade-offs are accepted in exchange for reliable asynchronous communication.

---

# Alternatives Considered

## Publish Directly to the Message Broker

Application features could publish Integration Events immediately after executing business logic.

### Advantages

-   Simpler implementation.
-   No background processing.

### Reasons Not Chosen

If the database transaction commits successfully but the message broker is unavailable, the business operation succeeds while the corresponding Integration Event is permanently lost.

This creates inconsistent state between modules.

---

## Distributed Transactions (Two-Phase Commit)

The database and the message broker could participate in a distributed transaction.

### Advantages

-   Strong consistency across resources.
-   Single transactional boundary.

### Reasons Not Chosen

Distributed transactions significantly increase operational complexity, reduce scalability, and are not universally supported across messaging technologies.

The architecture instead favors eventual consistency with reliable delivery.

---

## Best-Effort Retry

Application features could attempt to publish directly to the broker and retry publication if it fails.

### Advantages

-   Relatively simple implementation.
-   No Outbox table.

### Reasons Not Chosen

Retries cannot guarantee reliable delivery.

If the application terminates after the database transaction commits but before publication succeeds, the Integration Event is permanently lost.

Reliable publication requires the event to be durably persisted before publication is attempted.

---

# Rationale

Reliable asynchronous communication should not depend on the availability of external infrastructure during a business transaction.

By separating in-process notification handling from asynchronous message delivery, JobWize allows business workflows to complete atomically while still guaranteeing reliable external publication. Notification handlers execute inside the originating transaction, whereas Outbox and Inbox processing provide reliable communication between modules and external systems.

This approach accepts eventual consistency in exchange for significantly improved reliability, resiliency, and fault tolerance.
