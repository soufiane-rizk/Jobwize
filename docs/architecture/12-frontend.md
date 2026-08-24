# Frontend Architecture

## Purpose

The frontend is a Blazor WebAssembly application built with MudBlazor. It provides the presentation layer for JobWize and communicates with the backend only through its HTTP API.

It owns user interaction, client-side presentation state, routing, and client-side validation. Business rules and persistence remain in the backend modules.

---

# Structure

```text
frontend/src/JobWize.Frontend
|
├── Modules
|   ├── Dashboard
|   └── Identity
|
└── Shared
    ├── Api
    ├── Authentication
    ├── Components
    ├── Layout
    ├── Navigation
    ├── Results
    └── Theme
```

`Modules` own feature pages, routes, feature services, and their registrations. `Shared` contains reusable, cross-cutting frontend infrastructure. A module contributes navigation through the shared navigation abstraction rather than editing the main layout directly.

---

# Application Composition and Routing

`Program.cs` is the frontend composition root. It registers MudBlazor, authentication services, the HTTP client, and each feature module.

`App.razor` owns the application-wide MudBlazor providers, authentication state cascade, router, protected-route behavior, and the global error boundary.

There are two layouts:

-   `MainLayout` is the authenticated application shell. It provides the app bar, navigation drawer, and main content area.
-   `AuthenticationLayout` is the full-screen anonymous shell used by login and other authentication pages.

Protected routes use `AuthorizeRouteView`. An anonymous visitor is redirected to the login route instead of seeing protected content.

---

# Authentication

Authentication tokens are stored locally through `ITokenStorage`. `JobWizeAuthenticationStateProvider` turns the access token into the Blazor authentication state and notifies the UI when a session starts or ends.

The named API client attaches the access token through `AuthenticationHandler`.

Login and registration save the returned access and refresh tokens only after a successful API result. Logout always clears the local session, even if the API request to revoke the server session fails; the UI surfaces that revocation failure as a warning.

When the API returns 401, `ApiService` clears the local session only if a token is already stored. This prevents an expected anonymous login failure from resetting the login page before it can display its inline error.

Refresh-token rotation and automatic access-token renewal are intentionally not implemented yet.

---

# API and Error Contract

Feature services inherit from `ApiService`. It sends HTTP requests using the shared API client and translates `application/problem+json` responses into the frontend `Result` type.

The backend error contract uses a `code` extension and, for validation failures, an `errors` extension containing field-level messages. Frontend code must preserve these details so forms can associate validation errors with their inputs.

`ApiService` does not display notifications. It handles only shared session behavior; pages decide how to present the result in their own context. Transport failures that cannot be represented as an API result are surfaced as `ApiRequestException`.

---

# UX Conventions

Use the presentation mechanism that matches the scope of the feedback:

-   Show field validation and expected form failures inline, near the form or input.
-   Use snackbars for transient, cross-page, or background-operation feedback, such as a partial logout failure.
-   Let `ApplicationErrorBoundary` show a safe recovery screen for unhandled UI errors. Do not render internal exception details to users.

MudBlazor providers are declared once at the application root. Layouts should focus on placement and navigation rather than registering global services or providers.

---

# Extending the Frontend

For a new business feature:

1. Create a module under `Modules` with its routes, pages, and API service.
2. Register the module from `Program.cs`.
3. Use the backend module's public contracts for request and response DTOs.
4. Add navigation through the shared navigation convention when the feature needs a visible entry point.
5. Keep business decisions in the backend; the frontend should present results and perform user-experience validation only.

---

# Current Scope

The shared frontend foundation and authentication flow are in place. The next planned milestone is refresh-token rotation and automatic renewal, followed by the first business feature.
