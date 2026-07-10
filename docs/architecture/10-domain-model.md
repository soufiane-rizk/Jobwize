# Domain Modeling

## Purpose

This document defines the conventions used when implementing the Domain layer in JobWize.

Its purpose is to ensure every module follows the same modeling principles, resulting in a consistent, maintainable, and expressive domain model across the entire application.

This document intentionally focuses on the project's conventions rather than explaining Domain-Driven Design (DDD) theory.

---

# Design Philosophy

JobWize follows a pragmatic Domain-Driven Design approach.

The Domain layer represents the business and is responsible for protecting its own consistency.

The goal is not to model every concept as a complex object, but to create a domain model that clearly expresses the business while remaining easy to understand and maintain.

The project follows these principles:

-   Business behavior belongs inside the Domain Model.
-   Domain Models protect their own invariants.
-   Favor clarity over theoretical purity.
-   Avoid unnecessary abstractions.
-   Introduce complexity only when it solves a real business problem.

---

# Domain Models

A Domain Model represents a business concept that owns both data and behavior.

Examples include:

-   User
-   Company
-   JobPosting
-   CandidateProfile
-   JobApplication

A Domain Model:

-   Owns its own state.
-   Exposes business behaviors.
-   Protects its invariants.
-   Never exposes public setters for business state.

The goal is to model business operations rather than exposing mutable data.

Good:

```csharp
user.Suspend();

user.ChangePassword(passwordHash);

user.Reactivate();
```

Avoid:

```csharp
user.Status = UserStatus.Suspended;

user.PasswordHash = hash;
```

---

# Entities

Every entity owns an identity.

All entities inherit from the shared `Entity` base class.

```text
Entity
└── Id
```

Not every entity is considered a Domain Model.

Supporting entities that belong to another Domain Model should inherit directly from `Entity`.

Examples include:

-   RefreshToken
-   CompanyMember
-   InterviewComment

---

# DomainModel

Top-level business models inherit from the shared `DomainModel` base class.

`DomainModel` extends `Entity` by providing common audit information.

```text
DomainModel
├── CreatedAt
├── UpdatedAt
└── DeletedAt
```

Examples include:

-   User
-   Company
-   CandidateProfile
-   JobPosting

Supporting entities that do not require auditing should inherit directly from `Entity`.

---

# Object Creation

Domain Models are created through static factory methods.

Constructors remain private to guarantee every instance starts in a valid state.

Example:

```csharp
var user = User.CreateCandidate(...);

var admin = User.CreateAdministrator(...);
```

Factory methods are responsible for creating valid Domain Models.

Technical concerns such as password hashing or token generation are performed outside the Domain layer before invoking the factory.

---

# Behaviors

Domain Models expose behaviors rather than setters.

Every state transition should occur through a business method.

Example:

```csharp
user.Suspend();

user.Reactivate();

user.ChangePassword(newPasswordHash);
```

This ensures every state change passes through the business rules defined by the Domain Model.

---

# Business Rule Responsibilities

Business rules belong to different layers depending on the information required to evaluate them.

## Application Layer

The Application layer is responsible for:

-   Request validation.
-   Authorization.
-   Loading Domain Models.
-   Coordinating multiple Domain Models.
-   Rules requiring external information.

Examples include:

-   User not found.
-   Email already exists.
-   Current user lacks permission.
-   Candidate already applied to a job.

Expected business failures return a `Result`.

---

## Domain Model

A business rule belongs to the Domain Model when it can be evaluated using only the model's own state.

Examples include:

-   A suspended user cannot be suspended again.
-   An active user cannot be reactivated.
-   A revoked refresh token cannot be revoked twice.

The Domain Model protects these invariants regardless of where it is used.

---

# Error Handling

JobWize distinguishes between expected business failures and invariant violations.

Expected business failures are represented using `Result`.

Invariant violations indicate incorrect usage of the Domain Model and throw a `BusinessRuleViolationException` containing one of the predefined business `Error` objects.

Technical failures continue to use standard exceptions.

This separation keeps normal business flow explicit while ensuring Domain Models remain protected against invalid state transitions.

---

# Value Objects

Value Objects are introduced only when they provide meaningful business value.

Typical reasons include:

-   Shared validation.
-   Shared behavior.
-   Expressing an important business concept.
-   Preventing duplicated logic.

Primitive types are preferred until a Value Object clearly improves the model.

The project intentionally avoids creating Value Objects for every primitive type.

---

# Domain Services

Most business behavior belongs inside Domain Models.

A Domain Service should only be introduced when business logic naturally belongs to the domain but cannot reasonably be assigned to a single Domain Model.

Domain Services should remain uncommon.

---

# Persistence Ignorance

The Domain layer should remain independent from persistence concerns.

Domain Models must not depend on:

-   Entity Framework Core.
-   Database access.
-   HTTP.
-   JSON serialization.
-   Messaging infrastructure.

The only acceptable persistence-related concession is supporting object materialization when required by the ORM.

---

# General Guidelines

When implementing Domain Models:

-   Expose behaviors instead of setters.
-   Keep constructors private.
-   Use static factory methods.
-   Protect business invariants.
-   Keep technical concerns outside the Domain layer.
-   Prefer primitive types until a Value Object provides clear value.
-   Avoid unnecessary abstractions.
-   Keep models focused on business behavior.
