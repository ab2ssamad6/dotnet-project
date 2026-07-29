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
| `Cors:AllowedOrigins` | `Cors__AllowedOrigins` | Allowed frontend origins. As an env var, pass one **comma-separated** string (`https://a.com,https://b.com`), no trailing slashes. |
| `Seed:*` | `Seed__*` | Default seeded account credentials; `Seed__SeedSampleData=false` skips the demo catalogue |
| `Security:UseHttpsRedirection` | `Security__UseHttpsRedirection` | `true` to force HTTPS. Leave `false` behind a TLS-terminating proxy (Railway, nginx) to avoid redirect loops. |
| `Security:UseForwardedHeaders` | `Security__UseForwardedHeaders` | `true` when hosted behind a proxy/load balancer, so `X-Forwarded-For`/`-Proto` are trusted (correct client IP for rate limiting) |
| — | `PORT` | Port the container listens on (Railway sets it); the entrypoint binds to it. Unset → 8080. |

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
   curl http://localhost:8080/health          # -> {"status":"healthy"}
   ```

   Swagger UI: <http://localhost:8080/swagger>. Log in with a [seeded account](#seeded-accounts).

5. **Stop the stack** when you're done:

   ```bash
   docker compose down          # stop and remove containers (keeps the mysql-data volume)
   docker compose down -v       # also delete the database volume for a clean slate
   ```

## Deploy to Railway (GHCR image + Railway MySQL)

Railway hosts both halves of this stack: a **MySQL 8** database service and the API as a **prebuilt
Docker image** pulled from the **GitHub Container Registry** (`ghcr.io`). Nothing is compiled on
Railway — the image is built by GitHub Actions (or by you locally) and Railway only runs it.

The image is already platform-ready: its entrypoint binds to `$PORT`, `/health` is an anonymous
health-check endpoint, and migrations + seeding run automatically on boot.

### What you need first

- Push access to this GitHub repository (the image is published under its owner: `ghcr.io/ab2ssamad6/lms-api`).
- A [Railway](https://railway.com) account.
- (Optional) An [Anam.ai](https://anam.ai) API key for the AI-trainer endpoint.

### Step 1 — Publish the image to GHCR

**Option A — GitHub Actions (recommended).** `.github/workflows/backend-image.yml` builds
`backend/` and pushes `ghcr.io/<owner>/lms-api:latest` plus a `sha-<short>` tag on every push to
`master` that touches `backend/`. It authenticates with the built-in `GITHUB_TOKEN`, so there is no
secret to configure. To publish without pushing code, run it manually: **Actions → Publish backend
image → Run workflow**.

**Option B — from your machine.** Needs a [classic PAT](https://github.com/settings/tokens) with the
`write:packages` scope:

```bash
cd backend
echo "$GHCR_PAT" | docker login ghcr.io -u <your-github-username> --password-stdin
docker build --platform linux/amd64 -t ghcr.io/ab2ssamad6/lms-api:v1 .
docker push ghcr.io/ab2ssamad6/lms-api:v1
```

`--platform linux/amd64` matters: Railway runs x86-64, and an image built on Apple Silicon without
it crashes with `exec format error`.

**Then decide the package's visibility.** A freshly published GHCR package is **private**. Either:

- make it public — repo → **Packages** → `lms-api` → **Package settings** → *Change visibility →
  Public* (Railway then needs no credentials); **or**
- keep it private and give Railway a PAT with `read:packages` when adding the image source (step 3).

### Step 2 — Create the project and the MySQL database

1. Railway dashboard → **New Project** → **Deploy MySQL** (or **New → Database → Add MySQL**).
2. Wait until the MySQL service is green. It provisions a ready-to-use database named `railway`, so
   there is no schema to create by hand — the API's migrations create the tables on first boot.
3. Open the service → **Variables** and note the name of the service (default: `MySQL`). You will
   reference it in step 4, so if you renamed it, use that name.

### Step 3 — Add the API service from the GHCR image

In the same project: **New → Docker Image** → paste the image reference:

```
ghcr.io/ab2ssamad6/lms-api:latest
```

- If the package is **private**, Railway prompts for registry credentials — username = your GitHub
  username, password = a PAT with `read:packages`.
- Keeping both services in one project matters: it puts them on the same private network, so the API
  reaches MySQL over `*.railway.internal` with no public exposure and no egress charges.

### Step 4 — Set the API service variables

Open the API service → **Variables** → **Raw editor**, and paste this (Railway resolves
`${{MySQL.*}}` references against the database service — rename the prefix if your MySQL service has
a different name):

```env
ASPNETCORE_ENVIRONMENT=Production
PORT=8080

ConnectionStrings__MySql=Server=${{MySQL.MYSQLHOST}};Port=${{MySQL.MYSQLPORT}};Database=${{MySQL.MYSQLDATABASE}};User=${{MySQL.MYSQLUSER}};Password=${{MySQL.MYSQLPASSWORD}};TreatTinyAsBoolean=true
Database__ServerVersion=8.0.36-mysql

Jwt__SigningKey=<paste: openssl rand -base64 48>
Jwt__Issuer=LmsApi
Jwt__Audience=LmsClient

Security__UseForwardedHeaders=true
Security__UseHttpsRedirection=false

Cors__AllowedOrigins=https://<your-frontend-domain>
AiTrainer__Anam__ApiKey=<your-anam-key>

Seed__SeedSampleData=true
Seed__AdminPassword=<a-strong-password>
Seed__TrainerPassword=<a-strong-password>
Seed__StudentPassword=<a-strong-password>
```

Notes on the non-obvious ones:

- **`Jwt__SigningKey` is required** and must be ≥ 32 characters; the value in `appsettings.json` is a
  placeholder that must not be used.
- **`Security__UseForwardedHeaders=true`** — Railway's edge proxy terminates TLS and forwards HTTP.
  Without this the per-IP rate limiter (100 req/min) sees only the proxy's address and throttles all
  visitors as one bucket. `Security__UseHttpsRedirection` must stay `false` for the same reason
  (a redirect loop otherwise).
- **`MYSQLHOST` is the private hostname** (`mysql.railway.internal`), which only resolves from inside
  the project. To reach the database from your laptop instead (e.g. a local `mysql` client), use the
  public proxy values Railway lists as `MYSQL_PUBLIC_URL` / `RAILWAY_TCP_PROXY_DOMAIN`.
- **`Database__ServerVersion`** must roughly match the MySQL image Railway runs (check its
  **Deployments** tab); any `8.0.x` value is fine for MySQL 8.
- **The seeded passwords are published in this README**, so override them on anything reachable from
  the internet. `Seed__SeedSampleData=false` additionally skips the demo catalogue *and* the demo
  users (whose shared `Demo#12345` password you cannot configure) — the three core accounts above are
  always created.

### Step 5 — Expose it and add the health check

1. API service → **Settings → Networking → Generate Domain**. When asked for the port, enter
   **8080** (matching `PORT` above). You get `https://<something>.up.railway.app`.
2. API service → **Settings → Deploy → Health Check Path**: `/health`. Raise **Health Check Timeout**
   to ~`300` seconds: the very first boot applies all migrations *and* seeds the demo catalogue
   (4 courses, ~132 quiz questions), which takes a while.

### Step 6 — Deploy and verify

Railway deploys as soon as the image and variables are set. Watch the API service's **Deploy Logs**
for `Now listening on: http://[::]:8080` and no `MySqlException`, then:

```bash
curl https://<your-service>.up.railway.app/health      # -> {"status":"healthy"}
```

Swagger UI: `https://<your-service>.up.railway.app/swagger`. Log in through `POST /api/auth/login`
with a [seeded account](#seeded-accounts).

### Step 7 — Point the frontend at it

Build `../frontend` with `VITE_API_BASE_URL=https://<your-service>.up.railway.app`, and put that
frontend's own origin in the API's `Cors__AllowedOrigins` (comma-separated, **no trailing slash**).
A missing origin surfaces as a browser CORS error while `curl` keeps working.

### Step 8 — Ship a new version

Railway does not poll the registry. After a new image is published:

- **Same tag (`latest`)** — API service → **Deployments** → **Redeploy** (or `railway redeploy` with
  the [CLI](https://docs.railway.com/guides/cli)) to pull the new digest.
- **New tag (`v2`, `sha-abc1234`)** — update the reference in **Settings → Source → Image**, which
  triggers a deploy on its own. This is the safer habit: the running version stays identifiable.

### Troubleshooting

| Symptom | Cause |
|---|---|
| Deploy healthy but the domain 502s | Domain's target port ≠ the port the app bound. Set `PORT=8080` and target port 8080. |
| `exec format error` in logs | Image built on ARM without `--platform linux/amd64`. |
| `denied` / `manifest unknown` when Railway pulls | GHCR package still private and no `read:packages` credentials attached. |
| `MySqlException: Unable to connect` on boot | MySQL service in a *different* project (no shared private network), or the `${{MySQL.*}}` prefix does not match the database service's name. As a fallback, swap the host/port for Railway's public proxy values (`RAILWAY_TCP_PROXY_DOMAIN` / `RAILWAY_TCP_PROXY_PORT`) — it works, but the traffic leaves Railway's network and is billed as egress. |
| Health check times out on the first deploy only | Migrations + demo seeding exceed the timeout — raise it, or set `Seed__SeedSampleData=false`. |
| Browser CORS error, `curl` fine | Frontend origin missing from `Cors__AllowedOrigins`, or it has a trailing slash. |
| Everything 429s | `Security__UseForwardedHeaders` not `true`, so all clients share the proxy's rate-limit bucket. |

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
