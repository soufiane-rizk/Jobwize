# Local Development

This document describes how to set up a local JobWize development environment.

## Prerequisites

Before starting, ensure the following tools are installed:

-   .NET SDK 10
-   Visual Studio (2022 or later), Visual Studio Insiders, JetBrains Rider, or another IDE capable of developing .NET 10 applications
-   Git

## Clone the repository

```bash
git clone https://github.com/soufiane-rizk/Jobwize.git
```

## Configure User Secrets

The application uses ASP.NET Core User Secrets to store confidential values required during local development.

Configure the JWT signing key for the API project:

```bash
dotnet user-secrets set "Jwt:SecretKey" "<your-secret>" --project backend/src/Api/JobWize.Api
```

The JWT signing key must be generated using a cryptographically secure random generator and must never be committed to the repository.

The `UserSecretsId` is already configured in the API project, so no additional initialization step is required.

## Run the application

Start the API:

```bash
dotnet run --project backend/src/Api/JobWize.Api
```

## Current local configuration

At the moment, local development requires:

-   ASP.NET Core User Secrets for confidential configuration.
-   `appsettings.json` for non-confidential application configuration.

As additional infrastructure is introduced (database, Docker, external services, etc.), this document will be updated with the required setup steps.
