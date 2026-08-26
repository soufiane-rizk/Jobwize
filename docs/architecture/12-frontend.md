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
    ├── Forms
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

`AuthenticationHandler` owns access-token renewal for every authenticated API request. On a 401 response it performs one refresh request through an anonymous API client, replaces the stored access and refresh tokens, and retries the original request once. Concurrent renewal attempts are serialized so token rotation cannot race.

The handler never refreshes login, registration, logout, or refresh requests. If renewal fails, it clears the local session; protected-route handling then redirects the user to login. This preserves inline errors for anonymous login failures.

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

# Forms

Forms own their client-side usability validation, such as required fields, format checks, and password confirmation. The backend remains authoritative for all business validation and returns field-level errors through the shared problem-details contract.

`ResultFormComponentBase<TResponse>` provides common submission state and access to server field errors. A form should keep its local validation rules explicit, then use `SubmissionResult` only for the result of submitting to the API. This prevents local validation from being coupled to server responses while allowing a field such as a duplicate email address to display the backend error inline.

The registration form keys its `MudForm` with `SubmissionVersion`. This makes MudBlazor reapply an unchanged server error after a repeated submission, rather than clearing it during its validation pass.

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

The shared frontend foundation and authentication flow, including candidate self-registration and automatic refresh-token rotation and renewal, are in place. The next onboarding work is SuperAdmin bootstrap and forced-password-change support; the first business feature follows.
