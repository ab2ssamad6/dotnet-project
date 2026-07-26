# LMS API — Training Management System

A production-ready **.NET 8** backend for a Learning Management System, built with ASP.NET Core
Minimal APIs, EF Core (MySQL), ASP.NET Identity + JWT, FluentValidation, AutoMapper, Serilog and
Swagger, following Clean Architecture.

## Architecture

```
src/
  Lms.Domain          Entities, enums, domain contracts (no external dependencies)
  Lms.Application     DTOs, service/repository interfaces, validators, AutoMapper, Result type
  Lms.Infrastructure  EF Core DbContext + configs + migrations + seed, Identity, JWT, services, Anam.ai client
  Lms.Api             Minimal API host: endpoints, middleware, auth, rate limiting, Swagger
tests/
  Lms.UnitTests        TokenService + validator unit tests
  Lms.IntegrationTests WebApplicationFactory smoke tests (auth flow + categories CRUD) on SQLite
```

Dependency direction: **Api → Infrastructure → Application → Domain**. The Application layer defines
interfaces; Infrastructure implements them.

## Features

- **Auth**: register, login, refresh (rotating tokens), logout, forgot/reset password, email
  verification, change password. Roles: **Administrator**, **Trainer**, **Student** with role-based
  authorization.
- **Catalog**: Categories, Trainings, Trainers, Modules (all CRUD), and polymorphic **learning
  activities** — Lessons, Exercises, Quizzes, Exams (quizzes/exams support Multiple Choice, Multiple
  Answers, and True/False questions).
- **Enrollment**: browse published trainings, enroll, track progress, complete modules, submit
  quizzes (auto-graded), certificate readiness (future-ready).
- **Admin dashboard**: counts (students, trainers, courses, modules, enrollments), trainings by
  category, and recent activity.
- **AI Trainer** (`IAITrainerService`): `StartSession` calls the real **Anam.ai** session-token
  endpoint; `AskQuestion` / `GetModulePresentation` / `StopSession` are defined abstractions (Anam.ai
  drives conversation client-side via its realtime SDK).
- **Cross-cutting**: global exception handling (RFC 7807 ProblemDetails), request/auth logging
  (Serilog), rate limiting, CORS, security headers, FluentValidation on every request.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL 8 (or use the provided Docker Compose)
- EF Core tools for migrations: `dotnet tool install --global dotnet-ef --version 8.*`

## Configuration

Settings live in `src/Lms.Api/appsettings.json`. Override secrets via environment variables (double
underscore = nesting) or user-secrets — **never commit real secrets**.

| Setting | Env var | Notes |
|---|---|---|
| `ConnectionStrings:MySql` | `ConnectionStrings__MySql` | MySQL connection string |
| `Jwt:SigningKey` | `Jwt__SigningKey` | **Required**, ≥ 32 chars. Placeholder must be replaced. |
| `Jwt:Issuer` / `Jwt:Audience` | `Jwt__Issuer` / `Jwt__Audience` | Token issuer/audience |
| `AiTrainer:Anam:ApiKey` | `AiTrainer__Anam__ApiKey` | Anam.ai API key (enables AI session endpoint) |
| `Cors:AllowedOrigins` | — | Allowed frontend origins |
| `Seed:*` | `Seed__*` | Default seeded account credentials |
| `Security:UseHttpsRedirection` | `Security__UseHttpsRedirection` | `true` to force HTTPS |

For local development, put secrets in `src/Lms.Api/appsettings.Development.json` (git-ignored) or use:

```bash
cd src/Lms.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:SigningKey" "<a-long-random-secret>"
dotnet user-secrets set "AiTrainer:Anam:ApiKey" "<your-anam-key>"
```

## Run with Docker Compose (recommended)

`docker-compose.yml` defines two services — **mysql** (MySQL 8, data persisted in the `mysql-data`
volume) and **api** (built from the `Dockerfile`). The API `depends_on` MySQL's health check, so on
startup it waits for the database, applies migrations, and seeds data automatically.

### Prerequisites

- [Docker Engine](https://docs.docker.com/engine/install/) 20.10+ with the Compose v2 plugin
  (`docker compose version`).
- (Optional) An [Anam.ai](https://anam.ai) API key to enable the AI-trainer session endpoint.

### Step by step

1. **Change into the backend directory** (where `docker-compose.yml` lives):

   ```bash
   cd backend
   ```

2. **Build the images and start the stack.** Pass your Anam.ai key so the AI-trainer endpoint works
   (omit it and that one feature stays disabled — everything else runs fine):

   ```bash
   ANAM_API_KEY=<your-anam-key> docker compose up --build -d
   ```

   - `--build` (re)builds the API image from source; drop it on later runs to reuse the cached image.
   - `-d` runs the containers in the background; omit it to stream logs in the foreground.
   - If host port **8080** is already taken, override it: `API_PORT=8085 ANAM_API_KEY=<key> docker compose up --build -d`
     (the API is then at `http://localhost:8085`).

   > The compose file ships **dev-only placeholder secrets** (`Jwt__SigningKey`, MySQL passwords).
   > Override them via environment variables before any real deployment.

3. **Wait for the API to become healthy.** The API blocks until MySQL's health check passes, then
   migrates and seeds. Follow the logs until you see it listening on port 8080:

   ```bash
   docker compose logs -f api
   ```

4. **Verify it's up.** Hit the health endpoint and open Swagger:

   ```bash
   curl http://localhost:8080/health          # -> Healthy
   ```

   Swagger UI: <http://localhost:8080/swagger>. Log in with a [seeded account](#seeded-accounts).

5. **Stop the stack** when you're done:

   ```bash
   docker compose down          # stop and remove containers (keeps the mysql-data volume)
   docker compose down -v       # also delete the database volume for a clean slate
   ```

## Run locally

```bash
cd backend
# Point at a running MySQL (see appsettings) then:
dotnet run --project src/Lms.Api
```

By default the app applies migrations and seeds on startup (skipped only in the `Testing` environment).

## Database migrations

```bash
cd backend
# Create a migration
dotnet ef migrations add <Name> --project src/Lms.Infrastructure --startup-project src/Lms.Api --output-dir Persistence/Migrations
# Apply migrations to the database
dotnet ef database update --project src/Lms.Infrastructure --startup-project src/Lms.Api
```

The initial migration (`InitialCreate`) is already included.

## Seeded accounts

| Role | Email | Password |
|---|---|---|
| Administrator | `admin@lms.local` | `Admin#12345` |
| Trainer | `trainer@lms.local` | `Trainer#12345` |
| Student | `student@lms.local` | `Student#12345` |

Sample categories, trainers and trainings (with modules and a quiz) are also seeded. Change these in
the `Seed` configuration section.

## Tests

```bash
cd backend
dotnet test
```

Unit tests cover the JWT token service and validators. Integration tests boot the API on an in-memory
SQLite database and exercise the full auth flow and category CRUD.

## AI Trainer / frontend

The sample `../frontend` app (vanilla JS + Anam.ai SDK) calls the API to obtain a streaming session
token. Two routes are available:

- `POST /api/ai-trainer/session` — authenticated; returns `{ sessionToken, provider, ... }`.
- `POST /api/session-token` — anonymous compatibility alias returning `{ sessionToken }` (matches the
  original prototype consumed by `frontend/script.js`).

Set `window.LMS_API_BASE` in the frontend if the API is not at `http://localhost:8080`, and ensure the
frontend origin is listed in `Cors:AllowedOrigins`.

## API surface (overview)

| Group | Base path | Auth |
|---|---|---|
| Authentication | `/api/auth/*` | anonymous (except change-password) |
| Categories | `/api/categories` | read: any; write: Admin/Trainer; delete: Admin |
| Trainers | `/api/trainers` | read: any; write: Admin/Trainer; create/delete: Admin |
| Trainings | `/api/trainings` | read: any; write: Admin/Trainer |
| Modules | `/api/trainings/{id}/modules`, `/api/modules` | read: any; write: Admin/Trainer |
| Activities | `/api/modules/{id}/activities`, `/api/activities`, `/api/modules/{id}/{lessons,exercises,quizzes,exams}` | read: any; write: Admin/Trainer |
| Enrollment | `/api/catalog`, `/api/enrollments/*` | Student |
| Dashboard | `/api/admin/dashboard` | Administrator |
| AI Trainer | `/api/ai-trainer/*`, `/api/session-token` | authenticated / anonymous alias |
| Health | `/health` | anonymous |

## Security notes

- JWT bearer auth with rotating refresh tokens; passwords hashed via ASP.NET Identity.
- Rate limiting (global 100/min; auth endpoints 10/min per IP), CORS, security headers
  (`X-Content-Type-Options`, `X-Frame-Options`, CSP, `Referrer-Policy`).
- EF Core parameterized queries (SQL-injection safe); FluentValidation on every request; ProblemDetails
  responses avoid leaking internals outside Development.
- **Dependency note:** `AutoMapper 13.0.1` carries advisory
  [GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x). It is used only for trusted
  internal entity→DTO mapping. AutoMapper 14+ resolves it but requires a commercial license; evaluate
  before upgrading, or replace with manual mapping.
