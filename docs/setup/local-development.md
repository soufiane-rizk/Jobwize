# Local Development

This document describes how to set up a local JobWize development environment.

## Prerequisites

Before starting, ensure the following tools are installed:

-   .NET SDK 10
-   Docker Desktop (or another Docker Engine compatible with Docker Compose)
-   Visual Studio (2022 or later), Visual Studio Insiders, JetBrains Rider, or another IDE capable of developing .NET 10 applications
-   Git

## Clone the repository

```bash
git clone https://github.com/soufiane-rizk/JobWize.git
```

## Start PostgreSQL

Start the local PostgreSQL container:

```bash
cd docker/development
docker compose up -d
```

Stop the container:

```bash
docker compose down
```

To remove the database and start with a clean state:

```bash
docker compose down -v
```

This command removes the PostgreSQL container and its persistent volume, resulting in a fresh database the next time the container is started.

The API is configured by default to connect to the local PostgreSQL instance using the connection string defined in:

```
backend/src/Api/JobWize.Api/appsettings.json
```

## Configure User Secrets

The application uses ASP.NET Core User Secrets to store confidential values required during local development.

Configure the JWT signing key for the API project:

```bash
dotnet user-secrets set "Jwt:SecretKey" "<your-secret>" --project backend/src/Api/JobWize.Api
```

The JWT signing key must be generated using a cryptographically secure random generator and must never be committed to the repository.

The `UserSecretsId` is already configured in the API project, so no additional initialization step is required.

## Apply database migrations

Database migrations are not applied automatically when the API starts. Apply them locally with:

```bash
dotnet ef database update --project backend/src/Modules/Identity/JobWize.Modules.Identity --startup-project backend/src/Api/JobWize.Api
```

The refresh-token hashing migration invalidates existing local refresh-token sessions. Log in again after applying it.

## Run the application

Start the API in one terminal:

```bash
dotnet run --project backend/src/Api/JobWize.Api
```

Start the Blazor WebAssembly frontend in another terminal:

```bash
dotnet run --project frontend/src/JobWize.Frontend
```

The frontend API base address is configured in:

```text
frontend/src/JobWize.Frontend/wwwroot/appsettings.json
```

Its origin must be allowed by the API CORS configuration. The local development origin is configured in:

```text
backend/src/Api/JobWize.Api/appsettings.Development.json
```

If either local URL changes, update the frontend `Api:BaseAddress` and the API `Cors:AllowedOrigins` entries so that they continue to match.

## Run the tests

From the repository root, run:

```bash
dotnet test
```

This executes the API, Runtime, and Shared unit-test projects.

## Current local configuration

Local development currently uses:

-   Docker Compose to run PostgreSQL.
-   ASP.NET Core User Secrets for confidential configuration (JWT signing key).
-   `appsettings.json` for non-confidential application configuration (database connection string, JWT issuer, audience, token lifetimes, logging, etc.).

As additional infrastructure is introduced (Redis, monitoring, message brokers, etc.), this document will be updated with the required setup steps.
