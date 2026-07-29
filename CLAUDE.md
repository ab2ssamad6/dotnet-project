# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository layout

- `backend/` — .NET 8 Learning Management System API (the main project; Clean Architecture).
- `frontend/` — a tiny vanilla-JS + Anam.ai WebRTC SDK demo (`index.html`, `script.js`) that fetches an AI-avatar session token from the API. Set `window.LMS_API_BASE` if the API is not at `http://localhost:8080`; its origin must be in `Cors:AllowedOrigins`.

## Toolchain (environment-specific)

The .NET 8 SDK is installed **user-local** at `~/.dotnet` (not on PATH by default; `sudo apt` has no password/TTY here). `dotnet` and `dotnet-ef` are symlinked into `~/.local/bin`. Key gotchas:

- `dotnet` works via the symlink. **`dotnet-ef` requires `export DOTNET_ROOT=/home/samad/.dotnet`** on each invocation, or it errors "You must install .NET to run this application."
- Non-interactive shells source no profile here, so exported PATH/env in `.bashrc`/`.profile` does not apply — set env inline per command.
- The shell sandbox **cannot reach Docker published ports or container IPs**. To exercise a running container, curl from inside the compose network: `docker run --rm --network backend_default curlimages/curl -s http://api:8080/health` (use `alpine` + `apk add curl jq` for JSON parsing).

## Common commands

All commands run from `backend/`.

```bash
dotnet build Lms.sln                 # build the solution
dotnet test Lms.sln                  # run all tests (14 unit + 11 integration)
dotnet test tests/Lms.UnitTests      # one project
dotnet test --filter "FullyQualifiedName~AuthFlowTests.Register_Login"   # single test / class
dotnet run --project src/Lms.Api     # run locally (needs a reachable MySQL per appsettings)

# EF Core migrations (note the DOTNET_ROOT export)
export DOTNET_ROOT=/home/samad/.dotnet
dotnet ef migrations add <Name> --project src/Lms.Infrastructure --startup-project src/Lms.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/Lms.Infrastructure --startup-project src/Lms.Api

# Full stack via Docker (MySQL 8 + API); API_PORT overrides the host port if 8080 is taken
ANAM_API_KEY=<key> API_PORT=8085 docker compose up --build -d
```

There is no linter/formatter configured beyond the compiler; `AutoMapper 13` triggers a known `NU1903` advisory warning on every build (see README security note) — it is expected, not a regression.

## Architecture

Clean Architecture with strict dependency direction **Api → Infrastructure → Application → Domain**. Application defines interfaces; Infrastructure implements them. Understanding these cross-cutting conventions requires reading several files:

- **Domain is dependency-free.** `ApplicationUser`/`RefreshToken` extend ASP.NET Identity and therefore live in `Infrastructure/Identity`, *not* Domain. Domain entities reference users by `Guid` FK only (e.g. `Enrollment.StudentId`, `Trainer.UserId`). All persisted aggregates derive from `Domain/Common/AuditableEntity` (Guid `Id` pre-set, `CreatedAt`/`UpdatedAt` stamped automatically in `LmsDbContext.SaveChanges`).

- **Services return `Result`/`Result<T>`** (`Application/Common/Result.cs`) with an `ErrorType`; endpoints map that to HTTP via `Api/Extensions/ResultExtensions.cs` (`ToHttpResult`, `ToCreatedResult`). Add new failure modes here, not as thrown exceptions.

- **Service implementations live in Infrastructure** (`Infrastructure/Services`), not Application, so they can use `LmsDbContext` directly. `Category`/`Trainer` use the generic `IRepository<T>` + AutoMapper; the rest query the DbContext directly and project explicitly (joined names, polymorphic shaping). AutoMapper is only for trivial entity→DTO maps (`Application/Mapping/MappingProfile.cs`).

- **Learning activities are EF Core TPH** (`Lesson`/`Exercise`/`Quiz`/`Exam` under abstract `LearningActivity`, with an intermediate abstract `Assessment` holding `Questions`). The discriminator column is **`ActivityKind`** (deliberately *not* named `ActivityType`, which is an ignored computed CLR property — naming them the same deletes the discriminator and breaks migration). See `Infrastructure/Persistence/Configurations/ActivityConfigurations.cs`.

- **Endpoints** are Minimal APIs grouped per feature in `Api/Endpoints/*`, aggregated by `EndpointExtensions.MapApplicationEndpoints`. Request validation is a per-endpoint filter: `.WithValidation<RouteHandlerBuilder, TRequest>()` resolves the FluentValidation validator from DI. Authorization uses role policies (`Administrator`, `Trainer`, `Student`, and `ContentManager` = Admin+Trainer) from `ApiServiceCollectionExtensions`.

- **AI Trainer** (`IAITrainerService` → `AnamAiTrainerService`, typed `HttpClient`): only `StartSessionAsync` calls the real Anam.ai `/v1/auth/session-token`. `AskQuestion`/`GetModulePresentation`/`StopSession` are defined abstractions (Anam.ai drives conversation client-side). `POST /api/session-token` is an anonymous compat alias for the frontend.

- **Startup** (`Api/Program.cs`): on boot (except in the `Testing` environment) it applies migrations and seeds via `DbInitializer`. Config binds `JwtOptions`/`AnamOptions`/`SeedOptions`. JWT bearer validation is wired through the **options pattern** (`AddOptions<JwtBearerOptions>().Configure<IOptions<JwtOptions>>`) so the signing key resolves at runtime — do not read the key eagerly from `IConfiguration` at registration time (it breaks test/config layering). The top-level `catch` must exclude `HostAbortedException` so EF design-time tooling works.

## Testing conventions

- Integration tests (`tests/Lms.IntegrationTests`) boot the real `Program` via `WebApplicationFactory<Program>` (there is a `public partial class Program` at the bottom of `Program.cs`) against **in-memory SQLite** created with `EnsureCreated` — the MySQL migrations are not applied there. The `Testing` environment disables startup migrate/seed; the factory seeds itself.
- SQLite's single in-memory connection is not concurrency-safe, so integration tests are forced **serial** via `[assembly: CollectionBehavior(DisableTestParallelization = true)]` (`AssemblyInfo.cs`). Keep that when adding tests.

## EF Core pitfalls proven in this codebase

When adding a child entity to an **already-tracked** parent's navigation collection, add it via the `DbSet` (`_context.X.Add(child)`), not only `parent.Children.Add(child)` — because `AuditableEntity` pre-sets the Guid key, EF's "IsKeySet" heuristic infers the child already exists and emits an UPDATE (0 rows → `DbUpdateConcurrencyException`). For progress/counts, prefer querying `ModuleCompletions` from the DB rather than reading the mutable tracked navigation. In LINQ-to-Entities projections use the `.Count()` **method** (not the `.Count` property) on navigations, and never `OrderBy` a member of an already-projected DTO (order before `Select`).

## Seeded accounts (dev)

`admin@lms.local` / `Admin#12345`, `trainer@lms.local` / `Trainer#12345`, `student@lms.local` / `Student#12345` (configurable under `Seed`). Secrets (real Anam key, JWT dev key) live only in the git-ignored `appsettings.Development.json`; production uses env vars.

## Demo data

On every non-`Testing` boot `DbInitializer` delegates to `Persistence/Seed/DemoData/DemoDataSeeder`, which seeds four fully written courses (Machine Learning Foundations, Building APIs with ASP.NET Core, Blockchain Fundamentals, Cyber Security Essentials — 6 modules each, 132 questions total), a Draft "Kubernetes in Practice", three extra trainers and three extra students (all `Demo#12345`), plus 8 enrollments with module completions and quiz attempts. Set `Seed:SeedSampleData=false` (env `Seed__SeedSampleData`) to start with an empty catalogue; the integration-test factory already does.

Each course is one file returning a pure `Training` graph, built through the helpers in `DemoData/DemoContent.cs` which assign module/activity `Order` from position — never number them by hand. Lesson `Content` is rendered as **plain text** (`whitespace-pre-line`, no markdown renderer), so use blank lines and `-` bullets only. Seeding is gated per course by title, per enrollment by `(StudentId, TrainingId)`, and per user by e-mail, so it converges rather than duplicating; a course found with *fewer* modules than the demo version is treated as old thin sample data and replaced (cascading away its enrollments). `docker compose down -v` gets a pristine database.
