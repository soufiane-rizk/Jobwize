# ADR-006 - Endpoints over MVC Controllers

## Status

Accepted

---

# Context

JobWize exposes its functionality through an HTTP API built on ASP.NET Core.

The HTTP layer should:

-   Align with Vertical Slice Architecture.
-   Keep business features independent.
-   Minimize coupling between unrelated endpoints.
-   Make adding, modifying, or removing endpoints straightforward.
-   Remain a thin adapter between HTTP and the Application layer.

Traditional MVC Controllers group multiple actions into a single class. As applications grow, these controllers tend to become collections of unrelated endpoints that evolve independently while sharing the same implementation file.

The architecture therefore requires an HTTP organization that follows business capabilities rather than technical grouping.

---

# Decision

JobWize organizes its HTTP layer using **one endpoint per application feature**.

Each endpoint is implemented as an independent class responsible for exactly one HTTP operation.

For example:

```text
POST /identity/login
        │
        ▼
Login.cs
```

or

```text
GET /companies/{id}
        │
        ▼
GetCompanyById.cs
```

An endpoint serves only as the HTTP adapter.

Its responsibilities are limited to:

-   Receiving the HTTP request.
-   Performing request binding.
-   Invoking the dispatcher.
-   Returning the HTTP response.

Business logic remains entirely within the corresponding application handler.

```text
HTTP Request
      │
      ▼
Endpoint
      │
      ▼
Dispatcher
      │
      ▼
Application Handler
      │
      ▼
Domain
```

This decision complements the Vertical Slice organization by ensuring that every HTTP endpoint maps directly to a single application feature.

---

# Consequences

## Positive

-   Every endpoint has a single responsibility.
-   Business capabilities remain independent.
-   Navigation becomes straightforward because each HTTP operation has exactly one entry point.
-   Endpoints remain small and focused.
-   Adding or removing a feature has minimal impact on unrelated code.
-   The HTTP layer remains a thin adapter over the Application layer.

## Trade-offs

-   The project contains a larger number of endpoint classes.
-   Developers unfamiliar with the approach may initially expect traditional MVC Controllers.

These trade-offs are accepted in exchange for improved modularity and feature ownership.

---

# Alternatives Considered

## MVC Controllers

A traditional ASP.NET Core application groups related HTTP actions inside controller classes.

Example:

```text
AuthController

- Login()
- Register()
- Logout()
- RefreshToken()
- ForgotPassword()
- ResetPassword()
```

### Advantages

-   Familiar ASP.NET Core programming model.
-   Well-known by most .NET developers.
-   Built into the framework.

### Reasons Not Chosen

As the application grows, controllers naturally accumulate unrelated actions that evolve independently.

This reduces cohesion and requires developers to navigate large files containing multiple business capabilities.

The architecture instead groups code by feature rather than by controller.

---

## Minimal APIs in Program.cs

Endpoints could be defined directly during application startup.

### Advantages

-   Minimal boilerplate.
-   Simple for very small applications.

### Reasons Not Chosen

As the number of endpoints increases, application startup becomes responsible for routing, making the composition root unnecessarily large and reducing maintainability.

---

## One Controller per Business Area

Another approach is to create smaller controllers for each business area.

Example:

```text
CompanyController

- Create()
- Update()
- Delete()
- GetById()
- Search()
```

### Advantages

-   Smaller controllers than a traditional monolithic controller.
-   Familiar organization.

### Reasons Not Chosen

Although controllers become smaller, they still group multiple independent business capabilities into a single class.

JobWize instead maps each HTTP endpoint directly to one application feature, preserving feature independence throughout the HTTP layer.

---

# Rationale

The HTTP layer exists only to translate HTTP requests into application requests.

By implementing one endpoint per feature, the architecture mirrors the Vertical Slice organization used throughout the application.

Each business capability has:

-   One endpoint.
-   One request.
-   One handler.
-   One clear execution path.

This keeps the HTTP layer simple, focused, and aligned with the overall architecture while allowing business logic to remain completely independent of the web framework.
