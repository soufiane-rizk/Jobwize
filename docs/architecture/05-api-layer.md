# API Layer

## Purpose

This document describes how HTTP requests are processed by the backend.

The API layer acts as the entry point of the application.

Its responsibility is to translate incoming HTTP requests into application commands or queries while remaining completely independent from business logic.

---

# Request Lifecycle

Every HTTP request follows the same lifecycle.

```mermaid
flowchart LR

    CLIENT["Blazor WebAssembly"]

    REQUEST["Contracts<br/>Request"]

    ENDPOINT["Endpoint"]

    COMMAND["Command / Query"]

    DISPATCHER["IDispatcher"]

    RESPONSE["Contracts<br/>Response"]

    CLIENT --> REQUEST
    REQUEST --> ENDPOINT
    ENDPOINT --> COMMAND
    COMMAND --> DISPATCHER
    DISPATCHER --> RESPONSE
    RESPONSE --> CLIENT

    classDef frontend fill:#dbeafe,stroke:#2563eb,color:#000;
    classDef contracts fill:#ede9fe,stroke:#7c3aed,color:#000;
    classDef application fill:#dcfce7,stroke:#16a34a,color:#000;

    class CLIENT frontend;
    class REQUEST,RESPONSE contracts;
    class ENDPOINT application;
    class COMMAND application;
    class DISPATCHER application;
```

The `IDispatcher` abstracts the underlying communication mechanism.

Application features remain unaware of how requests and notifications are executed. The configured Runtime Execution Model determines the underlying communication strategy while exposing a single programming model through `IDispatcher`.

---

# Responsibilities

The API layer is intentionally thin.

Its responsibilities are limited to:

-   Receiving HTTP requests.
-   Creating the corresponding Command or Query.
-   Dispatching the request through `IDispatcher`.
-   Returning the appropriate HTTP response.

Business logic must never be implemented inside the API layer.

---

# Endpoints

Each application feature owns its HTTP endpoint.

Endpoints are implemented using ASP.NET Core Minimal APIs.

For example:

```text
Application
└── Users
    └── CreateUser.cs
```

The endpoint is responsible only for translating the HTTP request into an application command or query.

Example:

```csharp
internal sealed class Endpoint : IEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", Handle);
    }

    private static async Task<IResult> Handle(
        Request request,
        IDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new Command(
            request.Email,
            request.FirstName,
            request.LastName);

        return await dispatcher.SendAsync(
            command,
            cancellationToken);
    }
}
```

Endpoints should remain lightweight.

They must not contain business logic.

---

# Requests and Commands

Although Requests and Commands often contain similar data, they represent different concepts.

A Request belongs to the transport layer.

A Command or Query belongs to the Application layer.

Keeping them separate allows the public API to evolve independently from the application's internal implementation.

Commands may eventually require additional information obtained from the current execution context without affecting the public HTTP contract.

---

# Current User

Business logic frequently requires information about the authenticated user.

Rather than passing this information through every Command, handlers access it through an application service.

Example:

```csharp
public interface IUserContext
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }
}
```

The implementation retrieves information from the current HTTP context while exposing a simple abstraction to the Application layer.

This keeps Commands focused on business intent while avoiding unnecessary duplication of contextual information.

---

# Feature Structure

Each application feature follows the same organization.

```text
CreateUser.cs

CreateUser
├── Command
├── Handler
├── Endpoint
└── Validator
```

Additional components, such as validators or mappings, may be introduced when required by the feature.

Keeping all implementation details together improves discoverability and reduces unnecessary file navigation.

---

# Relationship with the Application Layer

The API layer is responsible only for accepting HTTP requests and dispatching them.

The execution of application use cases—including execution pipelines, command handling, business orchestration, and response generation—is described in **11 - Application Layer**.

---

# Error Responses

Application failures are returned as RFC 7807-style problem details with the content type `application/problem+json`.

Every failure contains:

-   `title` — a stable, user-safe category label.
-   `status` — the HTTP status code.
-   `detail` — the error message.
-   `code` — the application error code.
-   `errors` — field-level validation errors when applicable.

`ResultExtensions.ToApiResult()` maps expected `Result` failures as follows:

| Error type | HTTP status |
| --- | --- |
| Validation | 400 Bad Request |
| Conflict | 409 Conflict |
| Not found | 404 Not Found |
| Unauthorized | 401 Unauthorized |
| Forbidden | 403 Forbidden |
| Unexpected or other failure | 500 Internal Server Error |

The frontend reads this contract and maps it back to its client-side `Result` abstraction. The `code` extension is therefore part of the API contract and must remain available when error handling evolves.

## Unexpected Exceptions

Unexpected exceptions are handled at two boundaries:

1. `ExceptionHandlingBehavior` converts an exception raised while dispatching a use case into the shared unexpected-failure result. This preserves the normal result contract for application execution.
2. `GlobalExceptionHandler` is the final HTTP safety net for exceptions outside the dispatcher scope, such as middleware or endpoint failures. It logs the exception and returns the same safe problem-details shape with a 500 status when the response has not already started.

Exception details must not be exposed to API consumers. Diagnostic information belongs in server-side logs.

---

# Design Principles

The API layer follows these principles:

-   Thin HTTP layer.
-   One endpoint per feature.
-   Endpoints never contain business logic.
-   Public contracts remain separate from application commands.
-   Communication occurs exclusively through `IDispatcher`.
-   The API layer remains independent of the configured Runtime execution model.
-   HTTP concerns remain isolated from the Application and Domain layers.
-   Feature-oriented organization.
