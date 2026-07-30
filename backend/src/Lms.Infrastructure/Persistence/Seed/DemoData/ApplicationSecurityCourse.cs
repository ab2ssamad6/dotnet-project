using Lms.Domain.Entities;
using Lms.Domain.Enums;

using static Lms.Infrastructure.Persistence.Seed.DemoData.DemoContent;

namespace Lms.Infrastructure.Persistence.Seed.DemoData;

public static class ApplicationSecurityCourse
{
    public const string Title = "Application Security for Developers";

    public static Training Create(Category category, Trainer trainer) => NewTraining(
        Title,
        "A defensive security course for the people who write the code. You will threat model a real service, "
            + "close the injection and cross-site scripting classes properly, build authentication and session "
            + "handling that survives review, get authorization right in a multi-tenant system, and secure the "
            + "dependencies, secrets and pipeline that ship it all to production.",
        DifficultyLevel.Advanced,
        category,
        trainer,
        ThreatModelling(),
        InjectionAndEncoding(),
        AuthenticationAndSessions(),
        AccessControl(),
        SupplyChainAndSecrets(),
        FinalAssessment());

    private static Module ThreatModelling() => NewModule(
        "Threat Modelling in an Afternoon",
        "Trust boundaries, STRIDE, and turning a diagram into a short list of controls worth building.",
        75,
        aiAvatarEnabled: true,
        NewLesson(
            "Draw the system, then find the boundaries",
            """
            Threat modelling answers four questions: what are we building, what can go wrong, what are we going
            to do about it, and did we do a good job. It does not require a specialist or a week - a whiteboard
            and ninety minutes with the people who know the system produces most of the value.

            Start with a data flow diagram, not an architecture diagram. Show the processes, the data stores,
            the external entities and the flows between them. Then draw the trust boundaries: every line where
            data crosses from something you control less to something you control more. The browser to your
            API is a boundary. Your API to the database is a boundary. A background worker consuming a queue
            that any tenant can write to is a boundary teams routinely miss.

            Boundaries are where controls live, because they are where assumptions change. Data that was
            validated inside your service becomes untrusted again the moment it has been through a queue, a
            cache, a third party or a user's browser. "It was validated upstream" is the sentence that precedes
            most injection bugs.

            Keep the model at the level of the feature you are shipping. A model of the entire platform is
            never updated and never read; a model of the new enrollment flow, drawn during design, is used.
            """),
        NewLesson(
            "STRIDE, and choosing what to fix",
            """
            STRIDE is a prompt list for the second question. Walk each element and flow of the diagram and ask
            about Spoofing (can someone claim to be who they are not), Tampering (can data be modified in
            transit or at rest), Repudiation (can an action be denied because nothing was logged), Information
            disclosure (can data leak to the wrong party), Denial of service (can a cheap request cause
            expensive work) and Elevation of privilege (can a user act beyond their role).

            The value is in the prompting, not the taxonomy. Teams that skip it consistently find injection and
            forget repudiation and denial of service entirely - unlogged administrative actions and an
            unbounded export endpoint are both classic, and both surface immediately once someone reads the
            letters aloud.

            Rank findings by realistic impact and likelihood, then decide honestly per item: mitigate with a
            control, eliminate by removing the feature or the data, transfer to a provider who does it better,
            or accept with a named owner and a date. An accepted risk that is written down and reviewed is a
            decision; an accepted risk that is silent is an incident waiting for a post-mortem.

            Finish by turning each mitigation into a work item and, where you can, a test. A regression test
            that asserts a user of tenant A gets a 404 for tenant B's record is worth more than a paragraph in
            a document, because it fails the build when someone refactors the repository layer.
            """),
        NewExercise(
            "Threat model one real feature",
            """
            Take a feature you are building, such as bulk enrollment import:

            1. Draw the data flow diagram and mark every trust boundary.
            2. Walk STRIDE across each flow and record at least eight concrete threats.
            3. Rank them by realistic impact and likelihood.
            4. For the top five, choose mitigate, eliminate, transfer or accept - and name an owner for each.
            5. Convert two mitigations into automated tests that fail if the control is removed.
            """,
            """
            A diagram with explicit boundaries, at least eight threats that name a specific flow rather than
            generic categories, five ranked decisions with owners, and two tests in the repository that fail
            when the control is disabled.
            """),
        NewQuiz(
            "Threat modelling check",
            Choice(
                "What is a trust boundary?",
                ("A point where data crosses between components with different levels of trust", true),
                ("The network perimeter of the production environment", false),
                ("The line between frontend and backend code repositories", false),
                ("The edge of the diagram", false)),
            Choice(
                "Which STRIDE category covers unlogged administrative actions?",
                ("Repudiation", true),
                ("Spoofing", false),
                ("Tampering", false),
                ("Denial of service", false)),
            TrueFalse(
                "Data validated earlier in a request can be treated as trusted after it has passed through a queue.",
                false),
            Multiple(
                "Which make a threat model useful rather than decorative?",
                ("Scoping it to the feature being designed", true),
                ("Recording accepted risks with an owner and a review date", true),
                ("Converting mitigations into automated tests", true),
                ("Producing a single platform-wide model updated annually", false))));

    private static Module InjectionAndEncoding() => NewModule(
        "Injection and the Browser",
        "Why parameterisation works, context-aware output encoding, and using CSP as a second layer.",
        80,
        aiAvatarEnabled: true,
        NewLesson(
            "Separate code from data",
            """
            Every injection vulnerability - SQL, OS command, LDAP, XPath, template - is the same mistake: a
            string containing both instructions and untrusted values, parsed by something that cannot tell them
            apart. The fix is always structural separation, never cleverer filtering.

            In SQL that means parameters. A parameterised query sends the statement and the values on separate
            channels, so the database plans the statement first and the value can never become syntax. It is
            not about escaping quotes; a parameter placeholder makes the attack unrepresentable. Blocklists of
            words like DROP or UNION fail because attackers have far more encodings than you have patterns, and
            they break legitimate input - the user surnamed O'Brien has done nothing wrong.

            The gaps in ORM safety are the parts that build SQL as text: raw query helpers, dynamic ORDER BY
            clauses, and table or column names interpolated from a request. Identifiers cannot be
            parameterised, so map them against an allowlist of known columns and reject anything else.

            For operating system commands, prefer an API over a shell entirely. If you must run a process, pass
            arguments as an array rather than a single command line, so no shell interprets metacharacters.
            The same principle covers templates: never build a template from user input, because template
            engines execute expressions by design.
            """),
        NewLesson(
            "Output encoding is contextual",
            """
            Cross-site scripting is an output problem wearing an input problem's clothes. The same string is
            harmless in one place and executable in another, so encoding must match the context it lands in:
            HTML body, HTML attribute, JavaScript, URL and CSS each require different escaping.

            Modern frameworks encode HTML by default, which is why the remaining bugs cluster around the
            escape hatches: dangerouslySetInnerHTML, v-html, direct innerHTML assignment, and templates
            injected into inline script. If user content must contain markup, sanitise it with a maintained
            library and an allowlist of tags and attributes. Do not write the sanitiser yourself; the mutation
            and encoding edge cases are numerous and they are attacked constantly.

            URLs need their own check. A link built from user input can carry a javascript: scheme, so validate
            that the parsed URL uses http or https before rendering it as an href.

            Content Security Policy is the second layer that limits the damage when encoding fails. A policy
            that disallows inline script and restricts sources turns many injections into a blocked console
            message. Deploy it in report-only mode first, fix what it flags, and prefer nonces or hashes to a
            broad unsafe-inline allowance - a policy containing unsafe-inline provides very little protection
            against exactly the attack it is meant to blunt.
            """),
        NewExercise(
            "Close an injection and XSS gap",
            """
            In a service you maintain:

            1. Find every place SQL is built as text, including dynamic sort and filter clauses.
            2. Parameterise the values and allowlist the identifiers.
            3. Find every raw-HTML escape hatch in the frontend and either remove it or sanitise with a library.
            4. Validate that user-supplied URLs use only http or https before rendering them.
            5. Add a report-only Content Security Policy and fix the first three violations it reports.
            """,
            """
            No remaining string-concatenated SQL values, an allowlist covering sortable columns, sanitised or
            eliminated raw-HTML sinks, and a CSP report log that is quiet enough to move to enforcing mode.
            """),
        NewQuiz(
            "Injection and XSS check",
            Choice(
                "Why does a parameterised query prevent SQL injection?",
                ("The statement is parsed before values arrive, so a value cannot become syntax", true),
                ("The driver escapes quotation marks in the value", false),
                ("The database rejects keywords such as UNION in parameters", false),
                ("Parameters are transmitted over an encrypted channel", false)),
            Choice(
                "How should a dynamic ORDER BY column from a request be handled?",
                ("Checked against an allowlist of known sortable columns", true),
                ("Passed as a query parameter like any other value", false),
                ("Escaped with the driver's string escaping function", false),
                ("Filtered for SQL keywords before use", false)),
            TrueFalse(
                "A Content Security Policy containing unsafe-inline still blocks most injected inline scripts.",
                false),
            Multiple(
                "Which are true of cross-site scripting defences?",
                ("Encoding must match the output context, such as attribute or JavaScript", true),
                ("Raw-HTML escape hatches need a maintained sanitiser with an allowlist", true),
                ("A single input filter at the API boundary removes the need to encode on output", false),
                ("URLs from user input should be checked for the http or https scheme", true))));

    private static Module AuthenticationAndSessions() => NewModule(
        "Authentication and Session Management",
        "Password storage, multi-factor authentication, session lifecycle and the token mistakes reviewers look for.",
        80,
        aiAvatarEnabled: false,
        NewLesson(
            "Credentials, storage and recovery",
            """
            Password storage has one rule: use a memory-hard algorithm designed for the job - Argon2id, scrypt
            or bcrypt - with a per-user salt and parameters tuned so verification takes a noticeable fraction
            of a second on your hardware. A general-purpose hash such as SHA-256 is fast, and fast is the
            attacker's ally when a dump is cracked offline.

            Composition rules have moved on. Current guidance favours length, a minimum around eight to twelve
            characters, screening against known-breached password lists, and no forced periodic rotation -
            rotation produces predictable sequences ending in a number that increments every quarter. Support
            long passphrases and paste from password managers; blocking paste is a security theatre that pushes
            people towards weaker, memorable choices.

            Multi-factor authentication is the highest-value control available, but the factors differ.
            Hardware keys and passkeys resist phishing because the credential is bound to the origin.
            Time-based codes are a large improvement over nothing and are still phishable in real time. SMS is
            the weakest, exposed to SIM swapping, and is better than no second factor at all.

            Account recovery is where authentication is usually broken, since it is a parallel login path that
            bypasses everything above. Reset tokens must be random, single-use, short-lived and invalidated on
            use, on password change and on a second reset request. Responses must not reveal whether an account
            exists, and a successful reset should terminate existing sessions so that stolen access does not
            survive the recovery it prompted.
            """),
        NewLesson(
            "Sessions, cookies and token pitfalls",
            """
            A session identifier is a bearer credential: whoever holds it is the user. Handle it accordingly.

            Cookie-based sessions remain a strong default for browser applications. Set HttpOnly so script
            cannot read the cookie, Secure so it never travels unencrypted, and SameSite - Lax or Strict - to
            blunt cross-site request forgery. Where SameSite is not sufficient, such as cross-origin form posts,
            add an anti-forgery token. Regenerate the session identifier on login and on privilege change, or
            an attacker who plants a known identifier before login inherits the authenticated session.

            Token-based schemes bring their own failure modes, and reviewers check the same list. Validate the
            signature with a fixed algorithm rather than trusting the token's own alg header, which is how the
            none-algorithm and confusion attacks succeed. Verify issuer, audience and expiry, not just the
            signature. Keep access tokens short-lived, because they are validated offline and cannot be
            revoked before they expire. Pair them with a refresh token that is stored server-side, rotated on
            each use, and revoked wholesale if a rotated token is replayed - reuse of an old refresh token is
            the clearest theft signal you will get.

            Storage matters more than the algorithm debate. A token in localStorage is readable by any script
            that gets injected, which turns a single XSS into full account takeover; an HttpOnly cookie is not.
            Finally, log out for real: invalidate server-side state rather than only deleting the client copy.
            """),
        NewExercise(
            "Audit an authentication flow",
            """
            Review the authentication in a service you have access to:

            1. Confirm the password hash algorithm and its parameters, and time one verification.
            2. Check the reset flow for single-use, short-lived tokens and for account enumeration in responses.
            3. Verify the session identifier is regenerated at login and that logout invalidates server-side state.
            4. Inspect cookie flags, and if tokens are used, confirm the algorithm, issuer, audience and expiry checks.
            5. Write up each gap with the exploitation path, not just the missing setting.
            """,
            """
            A findings list where every item explains what an attacker would do with it, verified reset-token
            invalidation on use and on password change, and session regeneration demonstrated by comparing the
            identifier before and after login.
            """),
        NewQuiz(
            "Authentication check",
            Choice(
                "Which algorithm family is appropriate for storing passwords?",
                ("A memory-hard function such as Argon2id, scrypt or bcrypt", true),
                ("SHA-256 with a per-user salt", false),
                ("AES encryption with a key held in the application", false),
                ("MD5 applied twice", false)),
            Choice(
                "Why must the session identifier be regenerated at login?",
                ("Otherwise an attacker who planted a known identifier inherits the authenticated session", true),
                ("To reset the cookie expiry clock", false),
                ("Because browsers reject reused identifiers", false),
                ("To keep the audit log ordered", false)),
            TrueFalse(
                "Replay of an already-rotated refresh token is a strong signal that the token family was stolen.",
                true),
            Multiple(
                "Which are correct handling of session cookies?",
                ("HttpOnly, so injected script cannot read the cookie", true),
                ("Secure, so it is never sent over plain HTTP", true),
                ("SameSite set to Lax or Strict to blunt cross-site request forgery", true),
                ("A long lifetime so users are not asked to sign in again", false))));

    private static Module AccessControl() => NewModule(
        "Authorization and Multi-tenancy",
        "Object-level checks, role and attribute models, and keeping one tenant out of another tenant's data.",
        75,
        aiAvatarEnabled: false,
        NewLesson(
            "Broken object-level authorization",
            """
            The most common serious flaw in modern applications is not injection; it is an endpoint that
            authenticates the caller and then forgets to check whether this caller may touch this object.
            GET /api/enrollments/{id} returns the record because the id was valid, not because it belonged to
            the requester.

            The fix is to make ownership part of the query rather than a separate check. Loading a record and
            then comparing its tenant identifier works until someone writes a second code path; scoping every
            query by the caller's tenant - and expressing that in a repository or a global query filter - makes
            the safe version the default and the unsafe version hard to write by accident.

            Choose the right failure response. Returning 403 for records that exist and 404 for records that do
            not tells an attacker which identifiers are real. For cross-tenant reads, 404 is usually the better
            answer.

            Never rely on unguessable identifiers as the control. Random UUIDs raise the cost of enumeration
            and they leak - through logs, referrer headers, support tickets and screenshots. They are a
            defence-in-depth measure, not authorization.

            Finally, check on every request. A permission evaluated at login and cached in a token stays true
            until the token expires, long after the role was revoked, and any UI-side check is a usability
            feature that an attacker simply skips by calling the API directly.
            """),
        NewLesson(
            "Roles, attributes and the mass assignment trap",
            """
            Role-based access control assigns permissions to roles and roles to users. It is simple to reason
            about and it covers coarse questions - may this user administer the catalog? It struggles with
            relationships, and teams patch that by inventing roles until there are forty of them, most held by
            one person.

            Attribute-based rules handle the relationship questions properly: a trainer may edit a training if
            they are the assigned trainer and it is not archived; a learner may view an enrollment if it is
            their own. In practice most systems want both - roles for the broad capability, attributes for the
            object-level decision - and want them expressed in one place rather than scattered through
            controllers.

            Deny by default. Authorization that is opt-in per endpoint fails the moment someone adds a new
            route and forgets the attribute; a pipeline that rejects unless a policy explicitly allows makes
            the omission a 403 in development rather than a breach in production.

            Mass assignment is the quiet elevation path. Binding a request body straight onto an entity lets a
            caller submit fields nobody exposed in the interface - role, tenantId, isApproved, ownerId - and
            the framework helpfully sets them. Bind to explicit request models containing only what the caller
            may set, and map deliberately onto the entity. The same discipline applies on the way out: return
            projections rather than entities, so internal fields are not serialised into a response because
            someone added a column.
            """),
        NewExercise(
            "Prove tenant isolation",
            """
            In a multi-tenant service:

            1. List every endpoint that accepts an object identifier.
            2. For each, confirm the query is scoped by the caller's tenant rather than checked afterwards.
            3. Write integration tests where tenant A requests tenant B's identifiers and expect 404.
            4. Attempt mass assignment against three endpoints by adding fields such as role or ownerId.
            5. Confirm responses expose projections rather than entities, and that permissions are re-evaluated per request.
            """,
            """
            A cross-tenant test per identifier-bearing endpoint, all returning 404, three mass assignment
            attempts that change nothing, and at least one gap found and fixed - most codebases have one.
            """),
        NewQuiz(
            "Authorization check",
            Choice(
                "An endpoint returns any record whose id is valid, for any authenticated caller. This is:",
                ("Broken object-level authorization", true),
                ("A mass assignment flaw", false),
                ("A session fixation flaw", false),
                ("An injection flaw", false)),
            Choice(
                "Why prefer 404 over 403 when a caller requests another tenant's record?",
                ("403 confirms the identifier exists, which helps enumeration", true),
                ("404 is faster to return", false),
                ("403 is reserved for authentication failures", false),
                ("Clients cache 404 responses", false)),
            TrueFalse(
                "Unguessable identifiers such as random UUIDs are a form of defence in depth, not authorization.",
                true),
            Multiple(
                "Which prevent mass assignment and over-exposure?",
                ("Binding to explicit request models containing only writable fields", true),
                ("Mapping deliberately from request model to entity", true),
                ("Returning projections instead of entities", true),
                ("Relying on the client not to send extra fields", false))));

    private static Module SupplyChainAndSecrets() => NewModule(
        "Dependencies, Secrets and the Pipeline",
        "Managing third-party risk, keeping secrets out of the repository, and hardening the build that ships your code.",
        75,
        aiAvatarEnabled: false,
        NewLesson(
            "Third-party code is your code",
            """
            Most of what runs in production was written by strangers. A dependency executes with your process
            privileges, reads your environment variables and reaches your network, and its transitive
            dependencies do the same - one direct package can pull in dozens you never evaluated.

            Manage it deliberately. Commit lockfiles so builds are reproducible and a compromised release
            cannot silently substitute itself. Run software composition analysis in CI and treat a known
            exploitable vulnerability in a reachable code path as a build failure, not a ticket. Generate an
            SBOM so that when the next widely-used library is found vulnerable you can answer within minutes
            whether you ship it, rather than starting an investigation.

            Reachability matters for triage. A critical advisory in a package used only by a build-time tool is
            not the same emergency as a moderate one in your request path, and treating every alert as
            equivalent guarantees that all of them get ignored.

            Watch installation-time risk. Post-install scripts run arbitrary code on developer machines and CI
            runners before any of your tests execute; disabling them where your toolchain allows removes a
            popular attack path. Prefer packages that are maintained, and be wary of names one character away
            from popular libraries - typosquatting works because the install output scrolls past.
            """),
        NewLesson(
            "Secrets and a pipeline that cannot be trivially subverted",
            """
            Secrets do not belong in the repository, and a secret that was ever committed is compromised even
            after the commit is removed - the history survives in clones, forks, caches and CI logs. Rotation is
            the only remediation; deletion is housekeeping.

            Load configuration secrets from the environment or a managed secret store, scope credentials
            tightly to the one thing they need to do, and rotate them on a schedule you actually keep. Add
            secret scanning to pre-commit hooks and to CI so the next accidental commit is caught in seconds
            rather than found by someone else later.

            The pipeline itself is production infrastructure with production credentials, and it is targeted
            because of it. Pin actions and build images to immutable digests rather than mutable tags, so a
            moved tag cannot change what runs. Give each job the minimum permissions it needs, and be
            deliberate about workflows triggered by untrusted pull requests - a workflow that checks out a
            fork's code and runs it with repository secrets is a straightforward path to exfiltration.

            Protect the branch that deploys: required review, required checks, no force-push. Sign or attest
            build artefacts so the environment can verify that what it runs is what your pipeline produced.
            Then log deployments and administrative actions to somewhere the deploying identity cannot edit,
            because the value of an audit trail is exactly its resistance to the person being audited.
            """),
        NewExercise(
            "Harden a repository and its pipeline",
            """
            Pick a real repository:

            1. Run composition analysis and triage findings by whether the vulnerable path is reachable.
            2. Generate an SBOM and confirm you can answer which release ships a given package version.
            3. Add secret scanning to pre-commit and CI, then scan the full history and rotate anything found.
            4. Pin every CI action or image to an immutable digest and reduce job permissions to the minimum.
            5. Review any workflow triggered by pull requests from forks for access to secrets.
            """,
            """
            A triaged dependency report distinguishing reachable from unreachable findings, a stored SBOM, a
            clean history scan with any exposed credential rotated, and a pipeline whose jobs run with least
            privilege on pinned digests.
            """),
        NewQuiz(
            "Supply chain check",
            Choice(
                "A credential was committed and later removed in a follow-up commit. It should be treated as:",
                ("Compromised, and rotated immediately", true),
                ("Safe, because the file no longer contains it", false),
                ("Safe once the branch is deleted", false),
                ("A low-priority hygiene issue", false)),
            Choice(
                "Why pin CI actions and build images to immutable digests?",
                ("A mutable tag can be repointed, changing what executes in your pipeline", true),
                ("Digests download faster than tags", false),
                ("Tags are deprecated by most registries", false),
                ("It removes the need for dependency scanning", false)),
            TrueFalse(
                "An SBOM mainly helps you answer quickly whether a newly disclosed vulnerability affects what you ship.",
                true),
            Multiple(
                "Which reduce supply chain risk?",
                ("Committing lockfiles so builds are reproducible", true),
                ("Failing the build on reachable, exploitable vulnerabilities", true),
                ("Disabling post-install scripts where the toolchain allows", true),
                ("Automatically upgrading every dependency to the latest release on each build", false))));

    private static Module FinalAssessment() => NewModule(
        "Final Assessment",
        "A timed exam covering threat modelling, injection, authentication, authorization and supply chain security.",
        45,
        aiAvatarEnabled: false,
        NewExam(
            "Application Security for Developers - Final Exam",
            Choice(
                "A trust boundary on a data flow diagram marks:",
                ("A point where data crosses between components of differing trust", true),
                ("The edge of the production network", false),
                ("The boundary between two teams' codebases", false),
                ("Any call that crosses a process boundary", false)).Worth(2),
            Choice(
                "Parameterised queries stop SQL injection because:",
                ("The statement is parsed before values arrive, so a value cannot become syntax", true),
                ("The driver escapes dangerous characters", false),
                ("The database blocks keywords inside parameters", false),
                ("Parameters are length-limited", false)).Worth(2),
            Choice(
                "A dynamic sort column supplied by the client should be:",
                ("Validated against an allowlist of sortable columns", true),
                ("Bound as a query parameter", false),
                ("Escaped using the driver's escaping helper", false),
                ("Accepted if it contains no SQL keywords", false)).Worth(2),
            Multiple(
                "Which are true of storing tokens in the browser?",
                ("A token in localStorage is readable by any injected script", true),
                ("An HttpOnly cookie cannot be read by page script", true),
                ("localStorage is safe provided the token is short-lived", false),
                ("Cookie-based sessions still need SameSite or anti-forgery tokens", true)).Worth(3),
            Choice(
                "An authenticated endpoint returns any record by id regardless of owner. The flaw is:",
                ("Broken object-level authorization", true),
                ("Session fixation", false),
                ("Mass assignment", false),
                ("Cross-site request forgery", false)).Worth(2),
            TrueFalse(
                "Permissions should be re-evaluated on each request rather than trusted from a token issued earlier.",
                true).Worth(2),
            Choice(
                "The correct remediation for a secret that was once committed is to:",
                ("Rotate it, because the history is already distributed", true),
                ("Remove the commit and consider it resolved", false),
                ("Add the file to the ignore list", false),
                ("Restrict repository access going forward", false)).Worth(2),
            Multiple(
                "Which controls harden a deployment pipeline?",
                ("Pinning actions and images to immutable digests", true),
                ("Granting each job the minimum permissions it needs", true),
                ("Keeping deployment logs where the deploying identity cannot edit them", true),
                ("Allowing fork pull requests to run workflows with repository secrets", false)).Worth(3)));
}
