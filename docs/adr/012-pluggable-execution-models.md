# ADR-012 - Pluggable Execution Models

## Status

Accepted

---

# Context

JobWize is designed as a Modular Monolith composed of independent business modules.

Application features communicate exclusively through the `IDispatcher`.

They never invoke transports, messaging systems, or other modules directly.

As the architecture evolved, a new requirement emerged.

The same application code should execute correctly regardless of how modules are deployed.

Today, all modules execute within the same process.

Tomorrow, one or more modules may execute as independent services.

The application layer should remain completely unaware of these deployment differences.

Communication semantics must remain stable while the underlying execution strategy is allowed to evolve.

---

# Problem

Without an execution abstraction, application code would become coupled to deployment decisions.

For example, publishing a notification might require the application layer to know whether it should:

-   Execute handlers directly.
-   Persist an Outbox message.
-   Publish to RabbitMQ.
-   Invoke another service over HTTP.
-   Send a gRPC request.

This would leak infrastructure concerns into business code and make future architectural evolution significantly more difficult.

---

# Decision

JobWize introduces the concept of an **Execution Model**.

The Dispatcher remains the single entry point for application communication.

Instead of deciding _how_ requests or notifications are executed, it delegates execution to the active `IExecutionModel`.

The Execution Model determines the appropriate execution strategy according to the current deployment topology.

```text
Application
      │
      ▼
 Dispatcher
      │
      ▼
Execution Model
      │
      ├── MonolithExecutionModel
      │
      └── DistributedExecutionModel
```

Application features remain completely independent from the selected execution model.

---

# Monolith Execution Model

When every module executes inside the same process, JobWize behaves as a true modular monolith.

Notifications are executed directly through in-process Notification Handlers.

```text
Handler
    │
    ▼
Dispatcher.PublishAsync(Notification)
    │
    ▼
MonolithExecutionModel
    │
    ▼
Notification Handlers
```

No serialization occurs.

No network communication occurs.

No message broker is involved.

The entire operation executes within the current application process.

---

# Distributed Execution Model

If one or more modules execute outside the current process, the execution model may choose an alternative communication strategy.

Possible implementations include:

-   RabbitMQ
-   HTTP
-   gRPC
-   Azure Service Bus
-   Kafka
-   Other transports

For example:

```text
Handler
    │
    ▼
Dispatcher.PublishAsync(Notification)
    │
    ▼
DistributedExecutionModel
    │
    ▼
Transport Strategy
    │
    ├── RabbitMQ
    ├── HTTP
    ├── gRPC
    └── ...
```

The application layer remains unchanged regardless of the selected transport.

---

# Execution Topology

The execution model is determined by the deployment topology rather than by application code.

As long as every module executes inside the same process, JobWize behaves as a modular monolith.

When one or more modules are extracted into independent services, the execution model may be replaced without modifying handlers, endpoints, domain models, or application services.

This allows the architecture to evolve gradually from a modular monolith toward a distributed system while preserving the programming model.

---

# Consequences

## Positive

-   Application code remains completely transport-agnostic.
-   Deployment decisions are isolated from business logic.
-   Modules can evolve independently from monolith to distributed services.
-   Communication strategies can evolve without modifying application features.
-   Infrastructure concerns remain encapsulated inside execution models.
-   The runtime gains full control over execution behavior.

## Trade-offs

-   Additional runtime infrastructure must be maintained.
-   Multiple execution models require additional testing.
-   Runtime configuration becomes responsible for selecting the appropriate execution model.

These trade-offs are accepted in exchange for long-term architectural flexibility.

---

# Alternatives Considered

## Hard-code In-Process Execution

Notifications could always execute directly inside the current process.

### Advantages

-   Simple implementation.
-   Minimal infrastructure.

### Reasons Not Chosen

This tightly couples the architecture to a modular monolith and makes future distribution considerably more difficult.

---

## Hard-code Distributed Messaging

All notifications could always be published through a message broker.

### Advantages

-   Consistent execution model.
-   Naturally supports distributed systems.

### Reasons Not Chosen

This introduces unnecessary serialization, infrastructure, and eventual consistency while every module already executes inside the same process.

The architecture should not pay the cost of distribution before distribution is required.

---

## Allow Application Code to Choose

Handlers could explicitly decide whether to execute locally or remotely.

### Advantages

-   Complete flexibility.

### Reasons Not Chosen

Application code would become coupled to infrastructure and deployment decisions.

Business logic should describe _what_ happens, never _how_ communication occurs.

---

# Rationale

The execution strategy of a request or notification is an infrastructure concern rather than a business concern.

By introducing pluggable Execution Models, JobWize separates communication semantics from communication mechanics.

Application features always communicate through the Dispatcher.

The active Execution Model determines how that communication is realized according to the current deployment topology.

This allows JobWize to behave as an efficient modular monolith today while providing a clear migration path toward independently deployed services in the future without requiring changes to application code.
