# JobWize Backend

Backend API for the JobWize platform.

The backend is built with ASP.NET Core and follows a modular monolith architecture.

---

# Requirements

-   .NET SDK 10.0+
-   Docker (for containerized execution)

---

# Running Locally

## Using Visual Studio

Open the solution:

```
backend/src/JobWize.slnx
```

Run the `JobWize.Api` project.

The API will start using the configured development settings.

---

# Running with Docker

## Build the image

From the repository root:

```bash
docker build -t jobwize-api ./backend
```

## Configure environment variables

Copy the example configuration:

```bash
cp backend/.env.example backend/.env
```

Update the values in `.env` with your local configuration.

## Run the container

```bash
docker run \
  --name jobwize-api \
  -p 8080:8080 \
  --env-file ./backend/.env \
  jobwize-api
```

The API will be available at:

```
http://localhost:8080
```

---

# Configuration

The backend uses ASP.NET Core configuration.

For container deployments, configuration values should be provided through environment variables.

See:

```
.env.example
```

for the required configuration values.

## Required Environment Variables

### Database

```
ConnectionStrings__Identity
```

PostgreSQL connection string used by the Identity module.

### JWT Authentication

```
Jwt__SecretKey
Jwt__Issuer
Jwt__Audience
```

JWT configuration used for token generation and validation.

---

# Health Checks

The API exposes health endpoints.

## Liveness

```
GET /health/live
```

Used to verify that the application process is running.

## Readiness

```
GET /health/ready
```

Used to verify that the application is ready to accept requests.

The readiness check verifies required dependencies such as database connectivity.

---

# Database

Database migrations are not automatically applied when the application starts.

Migrations should be handled by deployment processes such as CI/CD pipelines.

---

# Docker Image

The Dockerfile uses a multi-stage build:

1. Restore project dependencies
2. Build the application
3. Publish the application
4. Run using the ASP.NET runtime image

The final image only contains the published application and the required runtime dependencies.
