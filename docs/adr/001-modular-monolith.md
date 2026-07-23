# ADR-001 - Modular Monolith Architecture

## Status

Accepted

---

# Context

JobWize is expected to grow into a large application composed of multiple business domains such as Identity, Companies, Applications, Notifications, and others.

The architecture must support long-term maintainability while avoiding unnecessary operational complexity during the early stages of the project.

The chosen architecture should:

-   Encourage strong business boundaries.
-   Support independent development of business domains.
-   Keep development, testing, and debugging simple.
-   Provide a clear evolution path if future scalability requirements change.

---

# Decision

JobWize is implemented as a **Modular Monolith**.

The application is deployed as a single ASP.NET Core application while being internally divided into independent business modules.

Each module owns its:

-   Business logic.
-   Database schema.
-   Contracts.
-   Integration events.
-   Background processing.

Modules communicate exclusively through well-defined contracts rather than direct implementation dependencies.

While all modules execute within a single process today, communication always occurs through the module runtime and dispatcher abstractions rather than direct method calls. The underlying execution model is selected by the runtime and may execute requests in-process or through a distributed transport without changing application code.

The modular monolith is considered the primary architecture of the system rather than a temporary step toward microservices.

Within the current deployment model, every module executes using the Monolith Execution Model, meaning all requests are dispatched inside the same process. The architecture intentionally separates the communication abstractions from the execution mechanism so that future execution models (such as HTTP, gRPC, or messaging) can be introduced for individual modules without changing the application layer.

Individual modules may be extracted into independently deployable services only when operational or organizational requirements justify doing so.

---

# Consequences

## Positive

-   Strong separation of business domains.
-   Clear ownership of business logic and data.
-   Easier local development and debugging.
-   Simple deployment and operational model.
-   Reduced infrastructure complexity compared to microservices.
-   Independent module extraction remains possible without redesigning the application architecture.
-   Communication mechanisms remain stable even if modules later move to different execution models.

## Trade-offs

-   The application is deployed as a single executable.
-   All modules currently share the same application process and therefore execute using the Monolith Execution Model.
-   Infrastructure failures may affect the entire application.
-   Individual modules cannot be deployed independently until they are extracted.

These trade-offs are considered acceptable given the current requirements and the benefits of maintaining a simpler operational model.

---

# Alternatives Considered

## Traditional Layered Monolith

A traditional layered architecture organizes the application into layers such as Controllers, Services, Repositories, and Infrastructure.

### Advantages

-   Familiar architecture for many developers.
-   Simple project organization.
-   Low initial complexity.

### Reasons Not Chosen

As applications grow, business boundaries tend to disappear as features become distributed across shared service and repository layers.

This often leads to increased coupling, reduced ownership of business logic, and a more difficult path toward future modularization.

---

## Microservices

A microservice architecture separates business domains into independently deployable services.

### Advantages

-   Independent deployment.
-   Independent scaling.
-   Autonomous teams.
-   Fault isolation between services.

### Reasons Not Chosen

Microservices introduce significant operational complexity, including distributed communication, service discovery, monitoring, versioning, and deployment orchestration.

These concerns are not justified by the current size or operational requirements of JobWize.

The project prefers to introduce this complexity only when it solves a concrete operational or organizational problem.

---

# Rationale

The Modular Monolith provides a balance between maintainability and operational simplicity.

It enables the application to be developed as a cohesive system while preserving clear business boundaries and reducing long-term coupling.

By designing modules as independent units from the beginning, and by separating communication abstractions from execution mechanisms, the architecture supports gradual evolution toward independently deployable services without requiring major architectural changes to business code.

This approach follows one of the guiding principles of JobWize:

> **Architectural complexity should be introduced only when it solves a concrete problem.**

---

# Related ADRs

-   ADR-002 – Vertical Slice Feature Organization
-   ADR-003 – Module Communication Strategy
-   ADR-004 – Reliable Integration Event Delivery with Outbox and Inbox
-   ADR-010 – Introduce a Custom Module Runtime
-   ADR-012 – Pluggable Execution Models
