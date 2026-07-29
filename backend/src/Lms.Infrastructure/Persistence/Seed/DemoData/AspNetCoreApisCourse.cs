using Lms.Domain.Entities;
using Lms.Domain.Enums;

using static Lms.Infrastructure.Persistence.Seed.DemoData.DemoContent;

namespace Lms.Infrastructure.Persistence.Seed.DemoData;

public static class AspNetCoreApisCourse
{
    public const string Title = "Building APIs with ASP.NET Core";

    public static Training Create(Category category, Trainer trainer) => NewTraining(
        Title,
        "Design, secure and ship production-ready REST APIs on .NET 8. You will build a real service end to "
            + "end: minimal API endpoints, model binding and validation, EF Core persistence, JWT authentication, "
            + "integration tests and a container image ready to deploy.",
        DifficultyLevel.Intermediate,
        category,
        trainer,
        GettingStarted(),
        RoutingAndValidation(),
        Persistence(),
        Security(),
        Delivery(),
        FinalAssessment());

    private static Module GettingStarted() => NewModule(
        "Minimal APIs and Project Setup",
        "The .NET 8 hosting model, the dependency injection container, configuration, and your first endpoint.",
        70,
        aiAvatarEnabled: true,
        NewLesson(
            "The hosting model",
            """
            An ASP.NET Core application is a host: an object that owns configuration, logging, dependency
            injection and the HTTP server, and controls the lifetime of all three.

            WebApplication.CreateBuilder(args) returns a builder in which you register services. Once you call
            builder.Build() the service collection is sealed and you switch to composing the middleware
            pipeline. That two-phase split explains most "cannot access a disposed object" and "service
            registered after build" errors beginners hit: everything that adds a service must happen before
            Build(), and everything that handles a request must happen after it.

            The request pipeline is an ordered chain of middleware. Each component receives the HttpContext,
            may act on it, and then either calls the next component or short-circuits. Order is behaviour, not
            style - authentication must run before authorization, and exception handling must be registered
            first so that it wraps everything after it.

            Three lifetimes govern injected services:

            - Singleton: one instance for the whole application.
            - Scoped: one instance per HTTP request. DbContext is scoped.
            - Transient: a new instance at every injection point.

            Injecting a scoped service into a singleton is the classic captive-dependency bug: the scoped
            object is captured forever and outlives the request it belonged to.
            """),
        NewLesson(
            "Your first minimal API",
            """
            Minimal APIs let you map a route straight to a lambda, with no controller class and no attribute
            routing. The framework inspects the delegate's parameters and figures out where each one comes
            from.

            app.MapGet("/api/trainings/{id:guid}", async (Guid id, ITrainingService service) => ...)

            Here id is bound from the route because the names match, and ITrainingService is resolved from the
            container because it is a registered service. Route constraints such as :guid or :int are part of
            matching, so a request to /api/trainings/not-a-guid produces a 404 rather than a binding error.

            Return values are translated for you. A plain object is serialised as JSON with a 200 status. To
            control the status code, return one of the TypedResults factories: TypedResults.Ok(dto),
            TypedResults.NotFound(), TypedResults.Created(uri, dto). Declaring the return type as
            Results<Ok<TrainingDto>, NotFound> keeps the endpoint strongly typed and lets OpenAPI document
            every outcome.

            Related endpoints are grouped with app.MapGroup("/api/trainings"), which lets you apply a common
            prefix, tags, filters and authorization to every endpoint in the group at once instead of
            repeating them.
            """),
        NewExercise(
            "Scaffold a trainings endpoint group",
            """
            Starting from an empty web project:

            1. Create a group at /api/trainings with the tag "Trainings".
            2. Add GET / returning a hard-coded list of two objects.
            3. Add GET /{id:guid} returning TypedResults.Ok for a known id and TypedResults.NotFound otherwise.
            4. Register a service in the container and inject it into the GET / handler.
            5. Confirm that /api/trainings/abc returns 404 rather than 400, and explain why.
            """,
            """
            Two endpoints answering under a shared prefix, the injected service resolving without any manual
            construction, and a clear explanation that the :guid route constraint fails the match before model
            binding ever runs.
            """),
        NewQuiz(
            "Getting started check",
            Choice(
                "Where must a service be registered for dependency injection to resolve it?",
                ("On builder.Services, before builder.Build() is called", true),
                ("On the WebApplication, after Build()", false),
                ("Anywhere, the container is mutable for the lifetime of the app", false),
                ("In the middleware pipeline, using app.Use()", false)),
            Choice(
                "Which service lifetime gives one instance per HTTP request?",
                ("Transient", false),
                ("Scoped", true),
                ("Singleton", false),
                ("Pooled", false)),
            TrueFalse(
                "Minimal APIs require a controller class to handle requests.",
                false),
            Choice(
                "A request to /api/trainings/not-a-guid on a route declared as {id:guid} returns:",
                ("400 Bad Request, because binding failed", false),
                ("404 Not Found, because the route constraint failed to match", true),
                ("500 Internal Server Error", false),
                ("200 OK with a default Guid", false)),
            Multiple(
                "Which statements about middleware ordering are correct?",
                ("Authentication must be registered before authorization", true),
                ("Exception handling should be registered early so it wraps later middleware", true),
                ("Order is a style preference and does not affect behaviour", false),
                ("Middleware can short-circuit by not calling the next component", true))));

    private static Module RoutingAndValidation() => NewModule(
        "Routing, Model Binding and Validation",
        "How values reach your handler parameters, and how to reject bad input before it does any damage.",
        85,
        aiAvatarEnabled: true,
        NewLesson(
            "Model binding sources",
            """
            Model binding fills your handler parameters from the request. Minimal APIs apply these rules in
            order:

            - A parameter whose name matches a route segment is bound from the route.
            - A simple type (string, int, Guid, DateTime, enum) that matches no route segment is bound from the
              query string.
            - A complex type is bound from the JSON request body. Only one parameter may bind from the body.
            - A type registered in the DI container is injected as a service.
            - Special types such as HttpContext, ClaimsPrincipal and CancellationToken are supplied by the
              framework.

            When the defaults are ambiguous, be explicit with [FromRoute], [FromQuery], [FromBody],
            [FromHeader] or [FromServices]. Explicit attributes are also the cheapest documentation a future
            reader gets.

            Always accept a CancellationToken and pass it down to every async call. When a client disconnects,
            the token is cancelled and your database work stops instead of burning connections on a response
            nobody will read.
            """),
        NewLesson(
            "Validation as a filter",
            """
            Validation belongs at the edge, before the handler body runs, so that the handler can assume its
            input is already well-formed.

            FluentValidation expresses rules as a class per request type:

            public class CreateTrainingValidator : AbstractValidator<CreateTrainingRequest>
            {
                public CreateTrainingValidator()
                {
                    RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
                    RuleFor(x => x.Duration).GreaterThan(0);
                }
            }

            An endpoint filter runs before the handler, resolves IValidator<TRequest> from the container,
            validates the bound request and returns 400 with a problem-details payload if it fails. Because it
            is a filter rather than code inside the handler, it applies uniformly and cannot be forgotten.

            Keep the two kinds of failure apart. Validation answers "is this request well-formed?" and returns
            400. Business rules answer "is this request allowed right now?" - a duplicate title, an unpublished
            training - and belong in the service layer, which reports them as a typed failure the endpoint maps
            to 409 or 422.
            """),
        NewExercise(
            "Add validation to a create endpoint",
            """
            1. Define CreateTrainingRequest with Title, Description, Duration and CategoryId.
            2. Write a validator: title required and at most 200 characters, duration greater than zero,
               category id not empty.
            3. Attach the validator through an endpoint filter rather than calling it inside the handler.
            4. POST an invalid payload and inspect the response body.
            5. Add a duplicate-title rule and decide where it belongs.
            """,
            """
            Invalid payloads produce 400 with a field-keyed error list and the handler never executes. The
            duplicate-title rule sits in the service, not the validator, because it needs a database lookup and
            expresses a business rule rather than a malformed request.
            """),
        NewQuiz(
            "Binding and validation check",
            Choice(
                "How many handler parameters may be bound from the JSON request body?",
                ("Exactly one", true),
                ("As many as you like", false),
                ("One per content type", false),
                ("None, the body must be read manually", false)),
            Choice(
                "A simple-typed parameter whose name matches no route segment binds from:",
                ("The request body", false),
                ("The query string", true),
                ("The request headers", false),
                ("The DI container", false)),
            TrueFalse(
                "Accepting a CancellationToken lets in-flight database work stop when the client disconnects.",
                true),
            Choice(
                "Rejecting a request because another training already uses that title is best modelled as:",
                ("A validation rule returning 400", false),
                ("A business rule in the service layer, mapped to 409", true),
                ("An unhandled exception", false),
                ("A route constraint", false)),
            Multiple(
                "Which are advantages of validating in an endpoint filter?",
                ("It runs before the handler body", true),
                ("It applies uniformly and cannot be forgotten", true),
                ("It removes the need for any server-side checks in services", false),
                ("It keeps validation rules out of the handler", true))));

    private static Module Persistence() => NewModule(
        "Persistence with EF Core",
        "Modelling data, writing queries that translate to good SQL, and evolving the schema with migrations.",
        100,
        aiAvatarEnabled: false,
        NewLesson(
            "DbContext and the change tracker",
            """
            A DbContext is a unit of work plus an identity map. It is registered as scoped, so each request
            gets its own, and it is not thread-safe: never share one across concurrent operations.

            When you query without AsNoTracking, every returned entity is tracked. Mutating a tracked entity
            and calling SaveChangesAsync produces an UPDATE for exactly the changed columns - you never write
            the UPDATE yourself. Read-only queries should opt out with AsNoTracking, which skips snapshotting
            and is measurably faster on large result sets.

            SaveChangesAsync wraps all pending changes in a single transaction. Either every insert, update and
            delete succeeds, or none of them do.

            Relationships are configured either by convention or explicitly in an IEntityTypeConfiguration.
            Delete behaviour deserves a deliberate decision per relationship: Cascade removes children with the
            parent, which is what you want for a training and its modules, while Restrict blocks the delete,
            which is what you want for a category that still has trainings pointing at it.
            """),
        NewLesson(
            "Queries that translate well",
            """
            LINQ against a DbSet is not LINQ against a list. The provider translates your expression tree into
            SQL, and anything it cannot translate either throws or silently falls back to evaluating in memory
            after pulling rows across the wire.

            Project early. Selecting into a DTO with Select emits a SELECT of just those columns and avoids
            materialising whole entity graphs:

            var items = await context.Trainings
                .AsNoTracking()
                .Where(t => t.Published)
                .OrderBy(t => t.Title)
                .Select(t => new TrainingListItem(t.Id, t.Title, t.Modules.Count()))
                .ToListAsync(ct);

            Three rules save most of the pain:

            - Order before you project. Sorting by a member of an already-projected type has no SQL equivalent.
            - On a navigation collection use the Count() method, not the Count property.
            - Use Include only when you genuinely need the tracked graph; a projection is usually better.

            Filter and paginate in the database with Where, Skip and Take. Calling ToListAsync first and then
            filtering pulls the whole table into memory - the single most common EF Core performance mistake.

            Schema changes go through migrations. dotnet ef migrations add <Name> diffs your model against the
            last snapshot and generates Up and Down methods; read the generated file before applying it,
            because a rename is often emitted as a drop plus an add, which would throw the data away.
            """),
        NewExercise(
            "Write a paginated projection",
            """
            1. Add a GET endpoint accepting page and pageSize query parameters.
            2. Query published trainings only, ordered by title, projected into a small DTO that includes the
               module count.
            3. Apply Skip and Take, and return the total count alongside the page.
            4. Enable EF Core query logging and read the generated SQL.
            5. Deliberately move OrderBy after Select and observe the failure.
            """,
            """
            One SQL statement per request that selects only the projected columns and applies LIMIT/OFFSET in
            the database. Moving OrderBy after Select fails because the sort key no longer exists in the
            translatable expression tree.
            """),
        NewQuiz(
            "EF Core check",
            Choice(
                "What does AsNoTracking() change?",
                ("Entities are not snapshotted, so changes are not detected and queries are faster", true),
                ("The query runs in a separate transaction", false),
                ("Navigation properties are not loaded", false),
                ("The query is cached across requests", false)),
            Choice(
                "In a LINQ-to-Entities projection over a navigation collection you should use:",
                ("The Count property", false),
                ("The Count() method", true),
                ("Length", false),
                ("Either works identically", false)),
            TrueFalse(
                "A single SaveChangesAsync call wraps all pending changes in one transaction.",
                true),
            Choice(
                "Deleting a category that still has trainings should normally:",
                ("Cascade and delete the trainings", false),
                ("Be restricted, so the delete fails", true),
                ("Set the trainings' CategoryId to a random value", false),
                ("Silently succeed and orphan the rows", false)),
            Multiple(
                "Which practices keep EF Core queries efficient?",
                ("Project into a DTO instead of materialising full entities", true),
                ("Apply Where and Take before ToListAsync", true),
                ("Call ToListAsync first and filter the list afterwards", false),
                ("Use AsNoTracking for read-only queries", true))));

    private static Module Security() => NewModule(
        "Authentication and Authorization",
        "Proving who the caller is with JWT bearer tokens, then deciding what they are allowed to do.",
        95,
        aiAvatarEnabled: false,
        NewLesson(
            "JWT bearer authentication",
            """
            Authentication establishes identity; authorization decides permission. They are separate steps and
            the middleware must run in that order.

            A JSON Web Token has three base64url segments: a header naming the algorithm, a payload of claims,
            and a signature. The signature is computed by the issuer over the first two segments using a key
            the API also holds. The API therefore validates the token entirely offline - no database lookup, no
            call back to the issuer. That is what makes bearer tokens scale, and also why a leaked token is
            usable until it expires.

            A JWT is signed, not encrypted. Anyone holding it can read the claims, so never put anything secret
            in the payload.

            Validation must check the signature, the issuer, the audience and the expiry. Turning off issuer or
            audience validation to "make it work" accepts tokens minted for a different application.

            Because tokens cannot be revoked, keep access tokens short-lived - minutes, not days - and pair
            them with a long-lived refresh token stored server-side. The refresh token can be revoked, rotated
            on each use, and tied to a device or session. Resolve the signing key through the options pattern
            rather than reading configuration eagerly at registration time, so that tests and environment
            layering work.
            """),
        NewLesson(
            "Roles, policies and ownership",
            """
            Once the token is validated, its claims become a ClaimsPrincipal on HttpContext.User.

            RequireAuthorization() demands any authenticated caller. Passing a policy name demands something
            more specific. Prefer named policies over scattering role strings through the codebase: a policy
            such as "ContentManager" can be defined once as "administrator or trainer" and later widened in a
            single place.

            options.AddPolicy("ContentManager", policy =>
                policy.RequireRole("Administrator", "Trainer"));

            Roles and policies answer "may this kind of user perform this kind of action?". They cannot answer
            "does this particular row belong to this particular caller?". Resource-based checks - a student
            reading only their own enrolments - need the entity in hand, so they belong in the service layer,
            comparing the caller's id claim against the row's owner and returning a forbidden result when they
            differ.

            Never trust an id supplied in the request body to identify the caller. Read it from the token.
            """),
        NewExercise(
            "Secure the enrolment endpoints",
            """
            1. Add JWT bearer authentication with validation of issuer, audience, lifetime and signing key.
            2. Define a policy allowing administrators and trainers, and apply it to the write endpoints.
            3. Leave the read endpoints open to any authenticated user.
            4. In the service, ensure a student can only read their own enrolments, using the id claim from the
               token rather than a value from the request.
            5. Verify that an expired token yields 401 and an under-privileged token yields 403.
            """,
            """
            Anonymous calls return 401, authenticated-but-wrong-role calls return 403, and a student requesting
            another student's enrolment is refused even though their token is perfectly valid.
            """),
        NewQuiz(
            "Security check",
            Choice(
                "What is the practical difference between 401 and 403?",
                ("401 means the caller is not authenticated; 403 means they are but lack permission", true),
                ("401 means the server is down; 403 means the route is missing", false),
                ("They are interchangeable", false),
                ("401 is for APIs and 403 is for web pages", false)),
            TrueFalse(
                "A JWT payload is encrypted, so it is safe to store secrets in its claims.",
                false),
            Choice(
                "Why are access tokens kept short-lived?",
                ("Because they are validated offline and cannot be revoked before they expire", true),
                ("Because long tokens exceed the header size limit", false),
                ("Because signature verification gets slower over time", false),
                ("Because the issuer must re-sign them hourly", false)),
            Choice(
                "Checking that a student may only read their own enrolment is best done:",
                ("With a role policy on the endpoint", false),
                ("In the service, comparing the caller's id claim against the row's owner", true),
                ("With a route constraint", false),
                ("In the JWT validation parameters", false)),
            Multiple(
                "Which must a correctly configured API validate on an incoming JWT?",
                ("The signature", true),
                ("The issuer and the audience", true),
                ("The expiry", true),
                ("That the caller's IP matches the one that requested the token", false))));

    private static Module Delivery() => NewModule(
        "Testing, Observability and Deployment",
        "Integration tests against the real host, logs and health checks worth reading, and a container image.",
        85,
        aiAvatarEnabled: false,
        NewLesson(
            "Integration testing the real host",
            """
            Unit tests are fast but prove little about an HTTP API: routing, model binding, filters,
            authentication and serialisation all live in the framework, and mocking them tests your mocks.

            WebApplicationFactory<Program> boots the actual application in-process and hands you an HttpClient
            wired to it, with no socket and no port. Requests travel the genuine middleware pipeline. For the
            generic Program type to be visible to the test project, Program.cs needs a "public partial class
            Program" declaration at the bottom.

            Point the tests at a database of their own. In-memory SQLite is a good compromise: it is a real
            relational engine that enforces constraints, and the schema can be created with EnsureCreated. Be
            aware that a single in-memory SQLite connection is not safe for concurrent access, so a suite that
            shares one connection must disable test parallelisation.

            Use a dedicated environment name for tests and skip startup migration and seeding in it, letting
            the fixture control the data instead. A test that depends on rows some other test created is a test
            that will fail on a different machine.
            """),
        NewLesson(
            "Observability and packaging",
            """
            Structured logging records events with named fields rather than interpolated strings:

            logger.LogInformation("Enrolled {StudentId} in {TrainingId}", studentId, trainingId);

            The message template stays constant, so a log backend can group every occurrence and let you filter
            by StudentId. Interpolating the values into the string throws that away.

            Expose a /health endpoint that checks the dependencies you cannot run without - the database first.
            Orchestrators poll it to decide whether an instance is ready for traffic, and a health check that
            always returns healthy is worse than none because it hides real outages.

            Return failures as problem details (RFC 7807), so clients get a consistent, machine-readable
            shape. Never return exception messages or stack traces to callers: log the detail, return a
            correlation id.

            Package with a multi-stage Dockerfile. The first stage uses the SDK image to restore, build and
            publish; the final stage copies only the published output into the much smaller ASP.NET runtime
            image. Copy the project files and restore before copying the rest of the source, so that Docker's
            layer cache reuses the restore whenever only code has changed. Configuration comes from
            environment variables at run time - double underscores map to nested keys, so
            ConnectionStrings__MySql sets ConnectionStrings:MySql - and secrets are injected by the platform,
            never baked into the image.
            """),
        NewExercise(
            "Test and containerise the API",
            """
            1. Add a test project referencing Microsoft.AspNetCore.Mvc.Testing.
            2. Build a factory that overrides the environment, swaps the database for in-memory SQLite and
               seeds a known fixture.
            3. Write a test that registers, logs in, and calls a protected endpoint with the returned token.
            4. Add a /health endpoint that verifies database connectivity.
            5. Write a multi-stage Dockerfile and confirm the final image does not contain the SDK.
            """,
            """
            A green test that exercises the real pipeline end to end, a health endpoint that turns unhealthy
            when the database is stopped, and a runtime image built from the ASP.NET base rather than the SDK.
            """),
        NewQuiz(
            "Delivery check",
            Choice(
                "What does WebApplicationFactory<Program> give a test?",
                ("An HttpClient wired to the real in-process host, with no socket or port", true),
                ("A mock of every registered service", false),
                ("A separate operating system process running the API", false),
                ("A generated OpenAPI client", false)),
            Choice(
                "Why prefer \"Enrolled {StudentId}\" over an interpolated string?",
                ("The template stays constant, so events can be grouped and filtered by field", true),
                ("It renders faster", false),
                ("Interpolated strings are not allowed in .NET 8", false),
                ("It avoids a compiler warning", false)),
            TrueFalse(
                "A multi-stage Dockerfile keeps the SDK out of the final runtime image.",
                true),
            Choice(
                "The environment variable ConnectionStrings__MySql maps to the configuration key:",
                ("ConnectionStrings:MySql", true),
                ("ConnectionStrings.MySql", false),
                ("ConnectionStrings/MySql", false),
                ("ConnectionStringsMySql", false)),
            Multiple(
                "Which make an integration suite reliable?",
                ("Each suite owns its data instead of depending on another test's rows", true),
                ("Startup migration and seeding are skipped in the test environment", true),
                ("Tests share one in-memory SQLite connection and run in parallel", false),
                ("A dedicated environment name isolates test configuration", true))));

    private static Module FinalAssessment() => NewModule(
        "Final Assessment",
        "A timed exam covering the whole course: hosting, binding, EF Core, security and delivery.",
        45,
        aiAvatarEnabled: false,
        NewExam(
            "Building APIs with ASP.NET Core - Final Exam",
            Choice(
                "Services must be registered on builder.Services:",
                ("Before builder.Build() seals the container", true),
                ("After Build(), while composing middleware", false),
                ("At any point during the application lifetime", false),
                ("Only inside a hosted service", false)).Worth(2),
            Choice(
                "Which lifetime is correct for a DbContext?",
                ("Scoped", true),
                ("Singleton", false),
                ("Transient", false),
                ("It must not be registered in DI", false)).Worth(2),
            Choice(
                "A complex-typed handler parameter is bound from:",
                ("The JSON request body", true),
                ("The query string", false),
                ("The route", false),
                ("The DI container", false)).Worth(2),
            Multiple(
                "Which belong in the service layer rather than in request validation?",
                ("Rejecting a title that duplicates an existing training", true),
                ("Refusing to enrol a student twice in the same training", true),
                ("Rejecting an empty title", false),
                ("Refusing a negative duration", false)).Worth(3),
            Choice(
                "Ordering a query by a member of an already-projected DTO fails because:",
                ("The sort key no longer exists in the translatable expression tree", true),
                ("EF Core forbids more than one OrderBy", false),
                ("Projections are always evaluated in memory", false),
                ("DTOs cannot implement IComparable", false)).Worth(2),
            TrueFalse(
                "A JWT is signed but not encrypted, so its claims are readable by anyone holding it.",
                true).Worth(2),
            Choice(
                "The main reason to pair a short-lived access token with a refresh token is:",
                ("Access tokens are validated offline and cannot be revoked before expiry", true),
                ("Refresh tokens are faster to validate", false),
                ("It halves the token size", false),
                ("The signing algorithm requires it", false)).Worth(2),
            Multiple(
                "Which are true of a multi-stage Dockerfile for this API?",
                ("The build stage uses the SDK image and the final stage uses the ASP.NET runtime image", true),
                ("Copying csproj files and restoring first improves layer caching", true),
                ("Secrets should be baked into the image so it runs anywhere", false),
                ("Configuration is supplied as environment variables at run time", true)).Worth(3)));
}
