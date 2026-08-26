# JobWize Project Context

## Purpose

This is a concise, living orientation guide for contributors and future work. It summarizes the current implementation and points to the authoritative architecture documents; it does not replace them.

Update it when a cross-cutting capability, project structure, or roadmap milestone materially changes.

---

# What JobWize Is

JobWize is an open-source career-management platform for organizing a job search. The system is a Blazor WebAssembly frontend backed by an ASP.NET Core modular monolith.

| Area | Current technology and role |
| --- | --- |
| Frontend | .NET 10 Blazor WebAssembly with MudBlazor |
| API | ASP.NET Core Minimal API |
| Backend architecture | Modular monolith, vertical slices, custom Runtime dispatcher |
| Persistence | Entity Framework Core and PostgreSQL |
| Contracts | Dedicated module contracts shared by API and frontend |

---

# Current State

Implemented shared capabilities:

-   Login, JWT authentication, protected routing, client authentication state, refresh-token rotation, automatic access-token renewal, and backend session revocation on logout.
-   Auth-aware application shell: theme, navigation drawer, app bar, anonymous authentication layout, and safe global UI error screen.
-   Unified backend error responses using `application/problem+json`, including application error codes and validation details.
-   Dispatcher exception handling plus a global HTTP exception safety net.
-   Unit tests for Runtime, Shared, and API cross-cutting behavior.

Current roadmap order:

1. Frontend shared foundation — implemented and ready to support feature work.
2. Refresh-token rotation, frontend token replacement, automatic access-token renewal, and expiry/failure handling — implemented.
3. First business feature — next.

---

# Architectural Rules That Matter Most

-   Modules own their domain model, persistence, schema, application logic, and public contracts. Do not access another module's implementation or database tables directly.
-   Endpoints are thin: transport request -> command/query -> `IDispatcher` -> `Result` -> HTTP response.
-   Expected failures use `Result`; unexpected dispatcher exceptions become the shared unexpected result. Exceptions outside dispatcher execution are handled by the API global exception handler.
-   API failures use problem details. Preserve the `code` extension and validation `errors` in client handling.
-   The frontend communicates only over HTTP and does not contain business rules.
-   The frontend owns feedback presentation: inline errors for forms, snackbars for transient cross-page/background feedback, and a safe global boundary for unhandled UI errors.
-   Local logout must always clear the client session, regardless of whether server-side refresh-token revocation succeeds.

---

# Where to Start Reading

| Need | Start here |
| --- | --- |
| Whole-system picture | [Architecture overview](architecture/00-overview.md) |
| Backend and module boundaries | [Backend overview](architecture/01-backend-overview.md), [module architecture](architecture/03-module-architecture.md) |
| Request/result/error flow | [API layer](architecture/05-api-layer.md), [application layer](architecture/11-application-layer-and-use-case.md) |
| Runtime dispatch | [Runtime](architecture/09-runtime.md) |
| Frontend conventions | [Frontend architecture](architecture/12-frontend.md) |
| Identity boundaries and planned capabilities | [Identity module](modules/Identity.md) |
| Local run and test commands | [Local development](setup/local-development.md) |

---

# Key Entry Points

| Concern | Entry point |
| --- | --- |
| API composition | `backend/src/Api/JobWize.Api/Program.cs` |
| HTTP exception fallback | `backend/src/Api/JobWize.Api/Exceptions/GlobalExceptionHandler.cs` |
| Result-to-HTTP mapping | `backend/src/Shared/JobWize.Shared/Endpoints/ResultExtensions.cs` |
| Dispatcher exception behavior | `backend/src/Shared/JobWize.Shared/Runtime/Behaviors/ExceptionHandlingBehavior.cs` |
| Frontend composition | `frontend/src/JobWize.Frontend/Program.cs` |
| Frontend routing/providers | `frontend/src/JobWize.Frontend/App.razor` |
| Shared API client behavior | `frontend/src/JobWize.Frontend/Shared/Api/ApiService.cs` |
| Authentication state | `frontend/src/JobWize.Frontend/Shared/Authentication/JobWizeAuthenticationStateProvider.cs` |

---

# Everyday Commands

From the repository root:

```bash
dotnet test
dotnet run --project backend/src/Api/JobWize.Api
dotnet run --project frontend/src/JobWize.Frontend
```

Before committing, follow the Conventional Commit format in [CONTRIBUTING.md](../CONTRIBUTING.md), for example: `docs(architecture): document frontend foundation`.
