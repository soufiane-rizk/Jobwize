# ADR-005 - Database per Module Schema

## Status

Accepted

---

# Context

JobWize is implemented as a Modular Monolith composed of independent business modules.

Although all modules share the same PostgreSQL database, each module must remain independent from a persistence perspective.

Without clear database ownership:

-   Business rules become tightly coupled.
-   Database changes affect unrelated modules.
-   Module boundaries gradually disappear.
-   Future extraction into independent services becomes significantly more difficult.

The architecture therefore requires a persistence strategy that preserves module autonomy while allowing all modules to coexist within the same physical database.

---

# Decision

Each business module owns its own PostgreSQL schema.

A module owns:

-   Its tables.
-   Its schema.
-   Its migrations.
-   Its indexes.
-   Its constraints.
-   Its Outbox.
-   Its Inbox.

Cross-module data access is performed exclusively through Module Queries and Integration Events executed by the custom module runtime.

Although all modules share the same physical database, they remain logically isolated through independent schemas and strict ownership rules.

A shared database **does not** imply shared data ownership.

```text
PostgreSQL Database

├── Identity Schema
│      Users
│      UserSessions
│      Inbox
│      Outbox
│
├── Companies Schema
│      Companies
│      CompanyMembers
│      Inbox
│      Outbox
│
└── Applications Schema
       JobApplications
       UserProjection
       CompanyProjection
       Inbox
       Outbox
```

The database belongs to the modules collectively, but every piece of data belongs to exactly one module.

---

# Consequences

## Positive

-   Business modules maintain complete ownership of their persistence model.
-   Database changes remain localized to the owning module.
-   Module boundaries remain explicit at the persistence level.
-   Independent schema evolution becomes possible.
-   Database migrations remain isolated between modules.
-   Cross-module coupling through persistence is significantly reduced.
-   Extracting a module into its own database context or independent service requires minimal architectural changes because the module already owns its complete data model.

## Trade-offs

-   Data cannot be shared through direct table access.
-   Cross-module joins are intentionally avoided.
-   Some data must be duplicated as projections.
-   Additional synchronization mechanisms are required between modules through Integration Events and local projections.

These trade-offs are accepted in exchange for stronger modular boundaries and long-term maintainability.

---

# Alternatives Considered

## Shared Database Model

A traditional monolithic application stores all tables within a shared database model where every module can freely access any table.

### Advantages

-   Simple querying.
-   Easy joins across business domains.
-   Less duplicated data.

### Reasons Not Chosen

Shared ownership gradually erodes module boundaries.

Business logic becomes coupled through persistence, making schema evolution increasingly difficult and preventing modules from evolving independently.

---

## Cross-Module Foreign Keys

Tables belonging to different modules could reference one another using foreign key constraints.

### Advantages

-   Strong referential integrity.
-   Simpler relational queries.

### Reasons Not Chosen

Foreign keys create compile-time and persistence-level coupling between modules.

Changes to one module's data model directly affect another module's persistence model.

Relationships between modules are instead represented through identifiers, projections, and well-defined application contracts.

---

## Independent Database per Module

Each module could own its own physical database from the beginning.

### Advantages

-   Complete physical isolation.
-   Independent database administration.
-   Simplifies a future microservice architecture.

### Reasons Not Chosen

The current operational requirements do not justify the additional infrastructure complexity.

A shared PostgreSQL database provides a simpler deployment model while preserving logical ownership through separate schemas.

If operational requirements change, modules can later be migrated to independent databases with minimal architectural changes because they already own their complete persistence model.

---

# Rationale

The architecture prioritizes **data ownership** over **physical separation**.

Modules are designed as though they already own independent datastores, even though they currently share the same PostgreSQL database.

This preserves strong modular boundaries while avoiding unnecessary operational complexity.

By separating ownership from physical deployment, JobWize remains simple to operate today while maintaining a clear evolution path toward independently deployed modules or microservices if future operational or organizational requirements justify that transition.

Because modules already own their schemas, DbContexts, migrations, and projections, separating a module into an independent database becomes primarily an operational change rather than an architectural rewrite.
