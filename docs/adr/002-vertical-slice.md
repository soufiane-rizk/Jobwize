# ADR-002 - Vertical Slice Feature Organization

## Status

Accepted

---

# Context

Each business module in JobWize is expected to contain a growing number of application features.

The internal organization of these features should:

-   Keep related code together.
-   Minimize navigation between files.
-   Encourage feature ownership.
-   Simplify refactoring and deletion.
-   Remain easy to understand for new contributors.

The project structure should prioritize cohesion around business capabilities rather than technical concerns.

---

# Decision

JobWize organizes application features using **Vertical Slice Architecture**.

Each feature represents a single business capability and contains everything required for its execution.

Whenever practical, an entire feature is implemented within a single source file.

For example:

```text
CreateUser.cs

• Command
• Validator
• Handler
• Endpoint
```

or

```text
GetUserById.cs

• Query
• Handler
• Endpoint
```

Keeping a feature in a single file allows developers to understand its complete behavior without navigating between multiple files.

The goal is to organize the application around business capabilities rather than technical layers.

## Feature Ownership

Each Vertical Slice owns the complete execution flow for a single business capability.

A feature is responsible for:

-   Endpoint
-   Command or Query
-   Validation
-   Handler

Cross-cutting concerns such as transactions, logging, metrics, and execution strategy are provided by the runtime rather than implemented inside the feature.

---

# Consequences

## Positive

-   Related code remains together.
-   Business capabilities are clearly isolated.
-   Navigation between files is significantly reduced.
-   Refactoring and feature removal become simpler.
-   New contributors can understand a feature by opening a single file.
-   Feature ownership becomes more explicit.

## Trade-offs

Some feature files may become relatively large.

This is considered an acceptable trade-off because the file still represents a single cohesive business capability.

Maintaining cohesion within a feature is preferred over splitting related code across multiple files solely to reduce file size.

---

# Alternatives Considered

## Layered Organization

A common approach is to organize the application by technical concern.

```text
Commands/
Queries/
Handlers/
Validators/
DTOs/
```

### Advantages

-   Familiar project structure.
-   Clear separation of technical responsibilities.

### Reasons Not Chosen

A single business feature becomes distributed across multiple folders, increasing navigation and making it more difficult to understand the complete implementation.

The project instead favors organizing code around business capabilities.

---

## Service Layer

Another common approach is to organize business logic into service classes such as:

```text
UserService
CompanyService
ApplicationService
```

### Advantages

-   Simple structure for small applications.
-   Familiar to many developers.

### Reasons Not Chosen

As applications grow, service classes tend to accumulate unrelated operations, reducing cohesion and making business logic harder to navigate.

JobWize does not use a centralized service layer as the primary organizational pattern.

This does **not** prevent the use of dedicated supporting services when reusable behavior is shared across multiple features.

Examples include:

-   Token generation.
-   File storage.
-   Image processing.
-   Resume parsing.
-   External integrations.

These services encapsulate reusable functionality without becoming the primary location for application features.

---

## One Folder per Feature

A common implementation of Vertical Slice Architecture stores each feature inside its own directory.

```text
CreateUser/

    Command.cs
    Handler.cs
    Validator.cs
    Response.cs
```

### Advantages

-   Features remain grouped together.
-   Better organization than technical layers.

### Reasons Not Chosen

Although related files are grouped, understanding a feature still requires navigating between multiple files.

JobWize instead places all closely related feature types into a single file whenever practical, reducing navigation while preserving cohesion.

---

# Rationale

JobWize adopts Vertical Slice Architecture because it organizes the application around business capabilities instead of technical layers.

The project extends this approach by favoring a single source file per feature whenever practical.

This decision reflects one of the guiding principles of the project:

> **A feature should be understandable by opening a single file while relying on the runtime for cross-cutting concerns such as dispatching, transactions, validation pipelines, and execution strategy.**
