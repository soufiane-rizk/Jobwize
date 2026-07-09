# ADR-009 — Persist Domain Models Directly Through EF Core Fluent Configuration

## Status

Accepted

---

## Context

Every business module contains a Domain layer responsible for implementing business behavior and enforcing business rules.

A common approach is to separate the Domain Model from the Persistence Model by introducing dedicated database entities and mapping between the two.

Another common approach is to use the same class for both purposes by placing Entity Framework attributes or persistence-related code directly inside the Domain Model.

Both approaches have advantages and disadvantages.

As JobWize follows a modular architecture with Vertical Slice Architecture and Entity Framework Core, we evaluated how Domain Models should be persisted while keeping the architecture simple and maintainable.

---

## Decision

JobWize persists Domain Models directly using Entity Framework Core.

The Domain Model is the object tracked by Entity Framework Core and represents both the business model and the persisted data.

Persistence configuration is **never** placed inside the Domain Model itself.

Instead, every Domain Model is configured externally through an Entity Framework Core Fluent Configuration class.

```
┌───────────────────────────────┐
│         Domain Model          │
│                               │
│ • Business Rules              │
│ • Invariants                  │
│ • State                       │
└───────────────┬───────────────┘
                │
                │ configured by
                ▼
┌───────────────────────────────┐
│     EF Core Configuration     │
│                               │
│ • Table Mapping               │
│ • Keys                        │
│ • Relationships               │
│ • Conversions                 │
└───────────────┬───────────────┘
                │
                ▼
┌───────────────────────────────┐
│          Database             │
└───────────────────────────────┘
```

The Domain Model remains responsible only for business behavior.

The EF Core configuration remains responsible only for persistence.

---

## Alternatives Considered

### Option 1 — Separate Persistence Model

```
Domain Model
        │
        ▼
Mapping Layer
        │
        ▼
Persistence Model
        │
        ▼
Database
```

#### Advantages

-   Complete separation between business model and persistence model.
-   Domain Model contains no persistence concerns.

#### Disadvantages

-   Requires mapping between two object graphs.
-   Increases boilerplate.
-   Every model evolution requires maintaining mappings.
-   Adds complexity without providing significant value for the project.

---

### Option 2 — Domain Model Wrapping a Persistence Model

```
Domain Model
        │
        ▼
Persistence Model
        │
        ▼
Database
```

The Domain Model wraps an internal persistence entity and exposes business behavior while delegating storage to the wrapped object.

#### Advantages

-   Business rules remain encapsulated.
-   Persistence entity cannot be modified directly outside the Domain Model.
-   Avoids mapping code.

#### Disadvantages

-   Introduces an additional abstraction layer.
-   Still requires maintaining two object types.
-   Adds indirection while providing little benefit when all layers live inside the same module project.

---

### Option 3 — Domain Model Persisted Through EF Core Fluent Configuration (Chosen)

```
Domain Model
        │
        ▼
EF Core Fluent Configuration
        │
        ▼
Database
```

#### Advantages

-   Single model representing business state.
-   No mapping layer.
-   Minimal boilerplate.
-   Business logic remains inside the Domain Model.
-   Persistence configuration is isolated from business logic.
-   Works naturally with EF Core change tracking.

#### Disadvantages

-   Domain Models are instantiated and tracked by Entity Framework Core.
-   The Domain Model structure must remain compatible with EF Core requirements.

---

## Consequences

Every Domain Model is persisted directly by Entity Framework Core.

Persistence metadata is defined exclusively through Fluent Configuration classes.

The Domain Model does not contain:

-   Entity Framework attributes.
-   Table definitions.
-   Column mappings.
-   Relationship configuration.
-   Database-specific annotations.

Repositories return Domain Models directly.

Application handlers manipulate Domain Models through business methods rather than modifying state directly.

This approach eliminates the need for persistence models and object mapping while keeping business behavior independent from persistence configuration.

---

## Rationale

Although the Domain Model is persisted directly by Entity Framework Core, it is **not** designed around the database.

The Domain Model remains a business object.

Entity Framework Core adapts itself to the Domain Model through Fluent Configuration, rather than the Domain Model adapting itself to Entity Framework Core.

This preserves a clear separation of responsibilities:

-   The Domain Model defines business behavior.
-   EF Core Fluent Configuration defines persistence.
-   The database stores the resulting state.

This approach provides a simple implementation model without introducing unnecessary abstraction layers while maintaining a clean separation between business logic and persistence concerns.
