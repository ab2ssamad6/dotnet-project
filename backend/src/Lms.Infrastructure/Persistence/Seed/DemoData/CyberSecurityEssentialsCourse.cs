using Lms.Domain.Entities;
using Lms.Domain.Enums;

using static Lms.Infrastructure.Persistence.Seed.DemoData.DemoContent;

namespace Lms.Infrastructure.Persistence.Seed.DemoData;

public static class CyberSecurityEssentialsCourse
{
    public const string Title = "Cyber Security Essentials";

    public static Training Create(Category category, Trainer trainer) => NewTraining(
        Title,
        "Defensive security for people who build and run software. You will learn to model threats before "
            + "writing code, store credentials properly, recognise and fix the OWASP Top 10, harden a network, "
            + "and run an incident from detection through to the lessons-learned review.",
        DifficultyLevel.Intermediate,
        category,
        trainer,
        Foundations(),
        Identity(),
        WebSecurity(),
        NetworkSecurity(),
        IncidentResponse(),
        FinalAssessment());

    private static Module Foundations() => NewModule(
        "Security Foundations and Threat Modeling",
        "The CIA triad, how risk is actually assessed, and using STRIDE to find threats before they ship.",
        60,
        aiAvatarEnabled: true,
        NewLesson(
            "The vocabulary that keeps discussions honest",
            """
            Security work goes wrong when the words are loose, so start with three properties every control
            ultimately serves.

            Confidentiality means only authorised parties can read the data. Integrity means data cannot be
            altered undetectably. Availability means authorised users can actually reach the system when they
            need it. Ransomware is instructive because it attacks availability by encrypting data, and
            increasingly confidentiality too by exfiltrating it first.

            Distinguish four terms that are routinely conflated. An asset is something worth protecting. A
            vulnerability is a weakness in it. A threat is an actor or event that could exploit that weakness.
            Risk combines how likely that is with how much it would cost: informally, risk = likelihood ×
            impact. This is why not every vulnerability is worth fixing immediately, and why a low-severity
            flaw on an internet-facing system can outrank a high-severity one that requires physical access.

            Keep authentication, authorization and accounting separate. Authentication establishes who you are.
            Authorization decides what you may do. Accounting records what you did. Conflating the first two is
            behind a large share of real-world access-control bugs.

            Several principles recur throughout the course:

            - Defence in depth. Assume any single control will fail, and ensure something else is behind it.
            - Least privilege. Grant the minimum access needed, for the minimum time.
            - Fail secure. When a component errors, it should deny rather than allow.
            - Reduce attack surface. Every open port, endpoint and dependency is something to defend.

            Security through obscurity - relying on attackers not knowing how something works - is not a
            control. A hidden endpoint with no authorisation check is unprotected; it is merely unlisted.
            """),
        NewLesson(
            "Threat modeling with STRIDE",
            """
            Threat modeling means asking how a design could be attacked, while it is still cheap to change.
            Done on a whiteboard before implementation, it is one of the highest-return security activities
            available.

            The process is four questions: What are we building? What can go wrong? What are we going to do
            about it? Did we do a good job?

            Start by drawing the system: components, data stores, and the flows between them. Mark the trust
            boundaries - the points where data crosses from less-trusted to more-trusted territory, such as
            browser to server, or application to database. Threats concentrate at those boundaries.

            STRIDE gives a checklist so you enumerate systematically rather than by imagination:

            - Spoofing: pretending to be someone else. Countered by authentication.
            - Tampering: unauthorised modification. Countered by integrity checks and signatures.
            - Repudiation: denying an action was taken. Countered by tamper-evident logging.
            - Information disclosure: exposing data to the wrong party. Countered by encryption and access
              control.
            - Denial of service: degrading availability. Countered by rate limiting, quotas and capacity.
            - Elevation of privilege: gaining rights you should not have. Countered by authorisation checks and
              least privilege.

            Apply the six categories to each element and flow. Then triage by risk rather than by category, and
            record accepted risks explicitly - a documented decision to accept a risk is a legitimate outcome,
            while an undocumented one is an accident waiting to be discovered during an incident.
            """),
        NewExercise(
            "Threat model a login endpoint",
            """
            Model POST /api/auth/login, which accepts an email and password and returns a JWT.

            1. Draw the components, flows and trust boundaries.
            2. Identify at least one threat in each STRIDE category.
            3. Propose one concrete mitigation per threat.
            4. Rank your threats by likelihood × impact.
            5. Name one risk you would consciously accept, and justify it.
            """,
            """
            Representative threats: credential stuffing using breached passwords (S), altering the token in
            transit without TLS (T), a user denying a password change with no audit log (R), a verbose error
            revealing whether an email exists (I), unthrottled login attempts exhausting the password hasher
            (D), and a token whose role claim is trusted without server-side verification (E). Mitigations
            include MFA, enforced TLS with HSTS, authentication event logging, uniform error responses, rate
            limiting with lockout, and validating the token signature and claims on every request. Credential
            stuffing and missing rate limits should rank highest: both are trivial to attempt and commonly
            successful.
            """),
        NewQuiz(
            "Foundations check",
            Choice(
                "Ransomware that encrypts files primarily attacks which property?",
                ("Availability", true),
                ("Integrity only", false),
                ("Non-repudiation", false),
                ("Authentication", false)),
            Choice(
                "In STRIDE, the E stands for:",
                ("Elevation of privilege", true),
                ("Encryption failure", false),
                ("Exposure of data", false),
                ("External dependency", false)),
            TrueFalse(
                "Keeping an endpoint's URL secret is an acceptable substitute for an authorisation check.",
                false),
            Choice(
                "Risk is best expressed as:",
                ("Likelihood combined with impact", true),
                ("The number of open vulnerabilities", false),
                ("The severity score alone", false),
                ("The cost of the security tooling", false)),
            Multiple(
                "Which are core defensive principles?",
                ("Defence in depth - assume any single control fails", true),
                ("Least privilege - minimum access, minimum duration", true),
                ("Fail secure - deny when a component errors", true),
                ("Trust anything originating inside the corporate network", false))));

    private static Module Identity() => NewModule(
        "Identity, Authentication and Access Control",
        "Storing credentials correctly, adding second factors, and the difference between OAuth and OIDC.",
        80,
        aiAvatarEnabled: true,
        NewLesson(
            "Storing passwords correctly",
            """
            Password storage is the one area where the correct answer is settled and the wrong answers are
            still everywhere.

            Never store passwords in a recoverable form. If your system can email a user their existing
            password, it is storing it reversibly and a database breach exposes every account.

            Do not use a fast general-purpose hash. SHA-256 is designed to be quick, and quick is exactly wrong
            here: commodity hardware computes billions of SHA-256 hashes per second, so a stolen table of
            unsalted hashes falls in hours. MD5 and SHA-1 are broken outright.

            Use a purpose-built password hashing function - Argon2id by preference, or bcrypt or PBKDF2 where
            it is not available. These are deliberately slow and memory-hard, with a tunable work factor you
            raise as hardware improves. A per-password random salt, stored alongside the hash, ensures two
            users with the same password produce different hashes and defeats precomputed rainbow tables.

            Length beats complexity. A long passphrase has more entropy than a short string mangled with
            symbols, and forced composition rules push users toward predictable substitutions. Modern guidance
            drops mandatory periodic rotation, which encourages incremental weakening, in favour of screening
            new passwords against known-breached lists and rotating on evidence of compromise.

            Two implementation details matter at the edges. Rate-limit authentication attempts, both per
            account and per source, or your slow hash becomes a denial-of-service amplifier. And compare
            secrets in constant time, so response timing does not leak how much of a value was correct.
            """),
        NewLesson(
            "MFA, OAuth 2.0 and OpenID Connect",
            """
            Multi-factor authentication requires evidence from more than one category: something you know, such
            as a password; something you have, such as a device or hardware key; something you are, such as a
            fingerprint. Two passwords are not two factors.

            The factors are not equally strong. SMS codes are the weakest common option, defeated by SIM
            swapping and interception, though still far better than nothing. Authenticator apps generating
            time-based codes are considerably stronger. Hardware security keys using WebAuthn or FIDO2 are the
            strongest widely available option, and uniquely they resist phishing: the key checks the origin, so
            a lookalike domain cannot obtain a usable response. Note that push-approval MFA is vulnerable to
            fatigue attacks, where an attacker repeats prompts until someone taps approve out of irritation.

            OAuth 2.0 and OpenID Connect are routinely confused. OAuth 2.0 is an authorization framework: it
            lets an application obtain limited access to a resource on a user's behalf, without receiving their
            password. It does not tell you who the user is. OpenID Connect is a thin identity layer on top,
            adding an ID token that carries authenticated identity claims. If you want "log in with", you want
            OpenID Connect.

            For interactive logins use the authorization code flow with PKCE. The browser receives a
            short-lived code rather than a token, and the code is exchanged for tokens in a step bound to a
            secret the client generated, so an intercepted code is useless on its own. The implicit flow, which
            returned tokens directly in the URL, is deprecated.

            Two persistent implementation errors are worth naming: accepting a token without validating its
            signature, issuer and audience, and reading the user's identity from a request parameter rather
            than from the verified token.
            """),
        NewExercise(
            "Rank four password implementations",
            """
            Rank these worst to best and state the specific flaw in each:

            A. Store the password encrypted with AES, key held in application configuration.
            B. Store MD5(password).
            C. Store SHA-256(salt + password), salt random per user.
            D. Store Argon2id(password) with a per-password salt and a tuned work factor.

            Then answer: why is A worse than C despite using real cryptography? And what single additional
            control most reduces the impact of any of these being breached?
            """,
            """
            Worst to best: B, A, C, D. B uses a broken, fast hash and no salt, so it falls to rainbow tables
            immediately. A is reversible by design - anyone obtaining the key recovers every password in
            plaintext, and keys in configuration leak with backups and repositories - which is worse than C,
            where the salt defeats precomputation even though SHA-256 remains far too fast for offline
            guessing. D is correct. The single highest-value additional control is MFA, which keeps a recovered
            password from being sufficient to authenticate.
            """),
        NewQuiz(
            "Identity check",
            Choice(
                "Why is bcrypt or Argon2id preferred over SHA-256 for password storage?",
                ("They are deliberately slow and tunable, making offline guessing expensive", true),
                ("They produce a longer digest", false),
                ("They are reversible with the correct key", false),
                ("They do not require a salt", false)),
            Choice(
                "The purpose of a per-password salt is to:",
                ("Ensure identical passwords hash differently, defeating precomputed tables", true),
                ("Encrypt the password before hashing", false),
                ("Allow the original password to be recovered", false),
                ("Shorten the stored hash", false)),
            Choice(
                "Which correctly distinguishes OAuth 2.0 from OpenID Connect?",
                ("OAuth 2.0 grants delegated authorization; OIDC adds authenticated identity on top", true),
                ("OAuth 2.0 authenticates users; OIDC only refreshes tokens", false),
                ("They are two names for the same specification", false),
                ("OIDC replaced OAuth 2.0 entirely", false)),
            TrueFalse(
                "Requiring a password plus a security question constitutes true multi-factor authentication.",
                false),
            Multiple(
                "Which strengthen an authentication system?",
                ("Rate limiting attempts per account and per source address", true),
                ("Screening new passwords against known-breached lists", true),
                ("Hardware security keys that verify the origin, resisting phishing", true),
                ("Forcing all users to change passwords every 30 days", false))));

    private static Module WebSecurity() => NewModule(
        "Web Application Attacks and Defenses",
        "The OWASP Top 10 in practice: broken access control, injection, XSS, CSRF and SSRF.",
        100,
        aiAvatarEnabled: false,
        NewLesson(
            "Broken access control and injection",
            """
            Broken access control is the most frequently exploited category in real applications, and the least
            glamorous. It means the server did not check that this caller may perform this action on this
            object.

            The common form is the insecure direct object reference: an endpoint such as
            GET /api/invoices/1043 that returns the invoice because the id exists, without checking it belongs
            to the caller. Changing the number to 1044 retrieves someone else's data. Sequential identifiers
            make enumeration trivial, though random ones only slow it - the missing check is the vulnerability.

            Three rules prevent nearly all of it. Enforce authorisation on the server for every request, since
            hiding a button changes nothing about what the API accepts. Derive the caller's identity from the
            verified session or token, never from a request parameter. And deny by default, so a new endpoint
            is inaccessible until it explicitly grants access.

            Injection happens when untrusted input is interpreted as code. In SQL injection, input concatenated
            into a query string changes the query's structure - the classic ' OR '1'='1 turning a login check
            into a tautology.

            The fix is parameterised queries. The SQL text and the data travel separately, so the database
            never parses user input as syntax. This is not a matter of escaping input well: escaping is
            error-prone and context-dependent, and a web application firewall filtering suspicious strings is a
            mitigation, not a fix. Use parameters, or an ORM that emits them, and keep the database account
            restricted to the privileges the application genuinely needs.

            The same principle applies beyond SQL. Passing user input to a shell command, an LDAP query or a
            template engine creates the same class of flaw, and the same answer applies: never let data become
            code.
            """),
        NewLesson(
            "XSS, CSRF and SSRF",
            """
            Cross-site scripting executes attacker-controlled JavaScript in a victim's browser, in the origin's
            context, giving it access to the DOM and any token reachable from script. Stored XSS persists the
            payload server-side and hits every viewer. Reflected XSS bounces it back from a crafted link.
            DOM-based XSS never involves the server, arising when client code writes untrusted input into a
            dangerous sink.

            The defence is contextual output encoding: encode data for the context it is rendered into - HTML
            body, attribute, JavaScript, URL - because the rules differ. Modern frameworks encode by default,
            so most real XSS comes from deliberately bypassing that, such as setting inner HTML directly. A
            Content Security Policy is a valuable second layer that limits which scripts may run, but it is
            defence in depth, not a substitute for encoding. Storing session tokens in cookies marked HttpOnly
            keeps script from reading them.

            Cross-site request forgery abuses the browser's habit of attaching cookies automatically. A user
            authenticated to your site visits an attacker's page, which submits a request to your endpoint; the
            browser includes the session cookie and the server sees an authentic-looking request the user never
            intended. The defences are an anti-forgery token the attacker's page cannot read, and SameSite
            cookie attributes that stop cookies being sent on cross-site requests. Note that APIs authenticated
            with an Authorization header rather than cookies are not exposed to classic CSRF, because nothing
            is attached automatically.

            Server-side request forgery tricks the server into making a request the attacker chooses. Because
            that request originates inside your network, it can reach internal services and cloud metadata
            endpoints that hold credentials. Defend with an allow-list of permitted destinations - never a
            deny-list, which redirects and alternate IP encodings defeat - by resolving and validating the
            address after redirects, and by blocking access to internal ranges.

            Finally, ship the security headers: a Content Security Policy, HSTS, X-Content-Type-Options set to
            nosniff, and a restrictive frame policy to prevent clickjacking.
            """),
        NewExercise(
            "Fix a vulnerable endpoint",
            """
            Review this handler:

            app.MapGet("/api/notes/{id}", async (int id, string userId, DbConn db) => {
                var sql = "SELECT * FROM Notes WHERE Id = " + id + " AND Owner = '" + userId + "'";
                var note = await db.QueryAsync(sql);
                return Results.Content($"<div>{note.Body}</div>", "text/html");
            });

            1. Identify three distinct vulnerabilities.
            2. For each, state the category and a concrete exploit.
            3. Rewrite the endpoint safely.
            4. Explain why fixing only the SQL leaves the endpoint exploitable.
            """,
            """
            SQL injection through concatenated input; broken access control, because the owner arrives as a
            request parameter the caller controls and can simply set to another user's id; and reflected XSS,
            since the note body is written into HTML unencoded. Exploits: a crafted userId terminating the
            string literal to return all rows; passing another user's id to read their notes; storing a script
            tag in a note body that runs when rendered. The fix uses a parameterised query, takes the owner
            from the authenticated principal rather than the query string, and returns JSON or encoded output.
            Fixing only the SQL leaves both the missing ownership check and the unencoded rendering intact.
            """),
        NewQuiz(
            "Web security check",
            Choice(
                "The correct primary defence against SQL injection is:",
                ("Parameterised queries, so input is never parsed as SQL syntax", true),
                ("Escaping quotes in user input", false),
                ("A web application firewall", false),
                ("Renaming database tables", false)),
            Choice(
                "Marking a session cookie HttpOnly prevents:",
                ("JavaScript from reading it, limiting theft via XSS", true),
                ("The cookie from being sent over HTTP", false),
                ("Cross-site request forgery entirely", false),
                ("The cookie from expiring", false)),
            Choice(
                "Returning /api/invoices/1044 to any authenticated caller is an example of:",
                ("Broken access control, specifically an insecure direct object reference", true),
                ("Cross-site scripting", false),
                ("Server-side request forgery", false),
                ("Security misconfiguration", false)),
            TrueFalse(
                "SSRF is dangerous largely because the request originates inside the trusted network.",
                true),
            Multiple(
                "Which genuinely mitigate cross-site request forgery?",
                ("An anti-forgery token the attacker's page cannot read", true),
                ("SameSite cookie attributes", true),
                ("Authenticating with an Authorization header instead of cookies", true),
                ("Accepting only POST requests", false))));

    private static Module NetworkSecurity() => NewModule(
        "Network and Infrastructure Security",
        "TLS, segmentation, zero trust, secrets management and keeping systems patched.",
        80,
        aiAvatarEnabled: false,
        NewLesson(
            "TLS and network segmentation",
            """
            TLS provides three things at once: confidentiality, so intermediaries cannot read the traffic;
            integrity, so they cannot alter it undetected; and server authentication, so the client knows which
            server it reached. That third property is what actually stops a machine-in-the-middle attack, and
            it depends entirely on certificate validation. Disabling certificate checks to make a client work
            removes the protection while leaving the encryption in place - a common and serious mistake.

            Use TLS 1.2 or 1.3 and disable older versions. Enable HSTS so browsers refuse to connect over plain
            HTTP after the first visit, closing the downgrade window. Encrypt internal traffic too: an attacker
            who has reached your network is exactly the one who benefits from plaintext between services.

            Segmentation limits what an intruder can reach after the first compromise, which is the assumption
            you should design around. A public web tier, an application tier and a database tier in separate
            segments, with firewall rules permitting only the necessary flows, means a compromised web server
            does not grant direct database access. Firewalls should default to deny, permitting known traffic
            explicitly, because a default-allow posture protects only what someone remembered to block.

            Zero trust extends this by rejecting the idea that network location implies trust. Every request is
            authenticated and authorised on its own merits, whether it comes from the internet or the office
            network. The traditional hard-perimeter model failed because a single phished laptop put the
            attacker inside, where everything was permissive.

            Distinguish detection from prevention. An intrusion detection system observes and alerts; an
            intrusion prevention system sits inline and blocks. Prevention is stronger and riskier, since a
            false positive drops legitimate traffic.
            """),
        NewLesson(
            "Secrets, patching and denial of service",
            """
            Credentials in source control are among the most reliably exploited weaknesses, because automated
            scanners watch public repositories continuously and act within minutes. Deleting the commit does
            not help: git retains history, and clones and forks are already distributed. Any secret that has
            been committed must be rotated, not merely removed.

            Keep secrets in a dedicated store - a cloud secret manager or a vault - and inject them at run time
            as environment variables or mounted files. Scope each credential narrowly, rotate on a schedule and
            immediately on suspicion, and keep them out of logs and error messages, which is a common accidental
            disclosure path.

            Patching is unglamorous and decisive. Most successful intrusions exploit known vulnerabilities for
            which a fix existed, sometimes for months. Maintain an inventory of what you run, because you
            cannot patch what you have forgotten. Scan dependencies continuously: modern applications carry
            hundreds of transitive packages, and a vulnerability three levels deep is still yours. Reduce
            surface by removing unused services, closing ports and uninstalling default packages.

            Denial of service comes in two shapes. Volumetric attacks flood capacity and are absorbed upstream
            by a provider with more bandwidth than the attacker. Application-layer attacks are subtler: a few
            requests aimed at an expensive operation - an unbounded search, an unpaginated export, a password
            hash - can exhaust a server while looking like ordinary traffic. Defend with rate limiting, quotas,
            timeouts, pagination limits and caching, and load-test the expensive paths deliberately.

            Finally, back up as though you will need it, because the ransomware playbook targets backups first.
            The 3-2-1 rule is three copies, on two media types, one off-site - and one that is offline or
            immutable. An untested backup is a hypothesis; restore drills are what make it a plan.
            """),
        NewExercise(
            "Segment a three-tier application",
            """
            An application has a public web tier, an internal API tier, a database, and an administrative
            interface used by staff.

            1. Assign each component to a network zone.
            2. Write the allowed flows as firewall rules, in the form: source zone → destination zone, port,
               purpose.
            3. State the default rule and justify it.
            4. Describe what an attacker who compromises the web tier can and cannot reach.
            5. Explain how zero trust would change your design.
            """,
            """
            A DMZ holds the web tier, a private application zone the API, a restricted data zone the database,
            and the admin interface is reachable only over VPN or an identity-aware proxy. Permitted flows:
            internet → DMZ on 443; DMZ → app zone on the API port only; app zone → data zone on the database
            port only; no path from DMZ to data. Default deny, so anything not explicitly permitted is blocked
            and forgotten services are not exposed by accident. A compromised web server reaches the API but
            not the database directly, converting a total breach into a contained one. Zero trust adds mutual
            authentication and authorisation between services, so reaching the API's port is not sufficient to
            use it.
            """),
        NewQuiz(
            "Infrastructure check",
            Choice(
                "The correct default posture for a firewall rule set is:",
                ("Deny by default, permitting known-required traffic explicitly", true),
                ("Allow by default, blocking known-bad traffic", false),
                ("Allow all internal traffic without restriction", false),
                ("Deny only traffic from outside the country", false)),
            Choice(
                "Disabling TLS certificate validation in a client:",
                ("Removes server authentication, allowing a machine-in-the-middle attack", true),
                ("Only disables encryption, leaving identity checks intact", false),
                ("Has no security consequence on an internal network", false),
                ("Improves security by avoiding expired-certificate failures", false)),
            TrueFalse(
                "A secret accidentally committed to git is adequately handled by deleting it in a later commit.",
                false),
            Choice(
                "Zero trust is best summarised as:",
                ("Network location grants no implicit trust; every request is authenticated and authorised",
                    true),
                ("Nobody in the organisation is trusted with any access", false),
                ("All traffic is blocked unless it comes from a VPN", false),
                ("Passwords are replaced entirely by biometrics", false)),
            Multiple(
                "Which mitigate application-layer denial of service?",
                ("Rate limiting and per-account quotas", true),
                ("Mandatory pagination limits on expensive queries", true),
                ("Timeouts on long-running operations", true),
                ("Increasing the password hashing work factor", false))));

    private static Module IncidentResponse() => NewModule(
        "Detection, Incident Response and Compliance",
        "Logging what matters, running an incident through the NIST lifecycle, and meeting reporting duties.",
        80,
        aiAvatarEnabled: false,
        NewLesson(
            "Logging and detection",
            """
            You cannot respond to what you cannot see, and most organisations discover breaches far later than
            they would like - frequently from an outside party rather than their own monitoring.

            Log the events that answer investigative questions: authentication successes and failures,
            authorisation denials, privilege and role changes, password and MFA changes, access to sensitive
            data, administrative actions, and configuration changes. Each entry needs a timestamp in UTC, the
            actor, the action, the target, the source address and an outcome. Include a correlation id so a
            single user journey can be reconstructed across services.

            Just as important is what must never be logged: passwords, session tokens, API keys, full payment
            card numbers, and personal data beyond what is necessary. Logs are widely readable, retained for a
            long time and shipped to third-party systems, so a secret in a log is a secret disclosed. This is a
            frequent accidental route for credential exposure, usually via verbose error handling.

            Centralise logs off the originating host. An attacker who compromises a machine will edit or delete
            its local logs, so append-only, off-host storage preserves the evidence. A SIEM aggregates and
            correlates these sources and raises alerts on patterns - impossible travel, a spike in
            authorisation failures, access at an unusual hour, data egress volumes outside the norm.

            Tune alerts deliberately. Alert fatigue is a real failure mode: a team receiving hundreds of
            low-value alerts daily stops reading them, and the one that mattered is missed. Fewer, higher-
            confidence alerts with clear runbooks beat exhaustive noise. Measure yourself on mean time to
            detect and mean time to respond rather than on alert volume.
            """),
        NewLesson(
            "Running an incident, and the rules you answer to",
            """
            Incidents are chaotic, so the value of a defined lifecycle is that nobody has to invent process
            under pressure. The widely used NIST phases are:

            Preparation. Runbooks, contact lists, defined roles, access to tooling, and rehearsal. Everything
            here must exist before the incident.

            Detection and analysis. Confirm something real is happening, establish scope and severity, and
            start a timeline. Assign an incident commander whose job is coordination and decisions, not
            hands-on investigation.

            Containment. Stop the bleeding - isolate hosts, revoke sessions and keys, block addresses, disable
            compromised accounts. Contain before eradicating, and preserve evidence while doing so. Powering a
            machine off destroys volatile memory that may be the only record of what ran; capture first where
            you can.

            Eradication. Remove the attacker's access and persistence: implants, added accounts, altered
            scheduled tasks, injected keys. Missing one persistence mechanism means the intrusion returns.

            Recovery. Restore from known-good backups, rebuild rather than clean where practical, and monitor
            closely for resumed activity before declaring normal service.

            Lessons learned. Within a couple of weeks, hold a blameless review producing concrete, owned
            actions with dates. Blameless is not decorative: teams that punish disclosure get slower reporting,
            and slow reporting is what turns an incident into a catastrophe.

            Regulation sets hard deadlines. Under GDPR, a personal data breach meeting the risk threshold must
            be reported to the supervisory authority within 72 hours of becoming aware, with notification to
            affected individuals when the risk to them is high. ISO 27001 certifies a management system for
            security; SOC 2 reports on controls against defined trust criteria. Compliance frameworks establish
            a floor, not a ceiling - a certified organisation with unpatched systems is compliant and
            vulnerable at the same time.
            """),
        NewExercise(
            "Write the first hour of a runbook",
            """
            Alerting shows a spike in successful logins from unusual locations across many accounts, consistent
            with credential stuffing. Some accounts show data exports.

            1. Write the first 60 minutes as ordered actions, grouped by NIST phase.
            2. State who does what, and who decides.
            3. Identify the evidence to preserve before containment changes anything.
            4. Decide whether this is reportable under GDPR, and justify it.
            5. Name two lessons-learned actions you would expect to result.
            """,
            """
            Detection and analysis first: confirm the pattern is not a load-balancer or VPN artefact, identify
            affected accounts, and open a timeline. Appoint an incident commander and a scribe, and preserve
            authentication logs, source addresses, session records and export audit trails before changing
            anything. Containment: revoke active sessions for affected accounts, force password resets, block
            offending sources, rate-limit authentication and temporarily restrict bulk export. This is
            reportable - personal data was accessed and exported, so the 72-hour clock starts at the point of
            awareness, and affected individuals need notifying where risk is high. Expected follow-ups include
            mandatory MFA and breached-password screening, plus alerting on anomalous export volume.
            """),
        NewQuiz(
            "Response check",
            Choice(
                "On confirming an active compromise, the immediate priority is to:",
                ("Contain it while preserving evidence", true),
                ("Wipe and rebuild the affected machines at once", false),
                ("Publish a public statement", false),
                ("Wait for the attacker to disconnect", false)),
            Choice(
                "Under GDPR, a qualifying personal data breach must be reported to the authority within:",
                ("72 hours of becoming aware", true),
                ("24 hours", false),
                ("30 days", false),
                ("The next annual audit", false)),
            TrueFalse(
                "Centralising logs off the originating host preserves evidence when that host is compromised.",
                true),
            Choice(
                "The 3-2-1 backup rule means:",
                ("Three copies, on two media types, with one off-site", true),
                ("Three daily backups, two weekly, one monthly", false),
                ("Three administrators, two approvals, one key", false),
                ("Three retries, two mirrors, one archive", false)),
            Multiple(
                "Which must never be written to application logs?",
                ("Passwords and session tokens", true),
                ("API keys and other credentials", true),
                ("Full payment card numbers", true),
                ("The UTC timestamp of the event", false))));

    private static Module FinalAssessment() => NewModule(
        "Final Assessment",
        "A timed exam covering threat modeling, credentials, web attacks, infrastructure and incident response.",
        45,
        aiAvatarEnabled: false,
        NewExam(
            "Cyber Security Essentials - Final Exam",
            Choice(
                "Risk is most usefully expressed as:",
                ("Likelihood combined with impact", true),
                ("The count of open findings", false),
                ("The highest CVSS score present", false),
                ("The security budget divided by asset value", false)).Worth(2),
            Choice(
                "The correct way to store user passwords is:",
                ("A slow, salted, purpose-built hash such as Argon2id or bcrypt", true),
                ("AES encryption with a key in application configuration", false),
                ("SHA-256 with a global salt", false),
                ("MD5 with a per-user salt", false)).Worth(2),
            TrueFalse(
                "A password plus a security question is multi-factor authentication.",
                false).Worth(2),
            Choice(
                "The primary defence against SQL injection is:",
                ("Parameterised queries", true),
                ("Escaping user input", false),
                ("A web application firewall", false),
                ("Restricting the database account's privileges", false)).Worth(2),
            Multiple(
                "Which are examples of broken access control?",
                ("An endpoint returning any record whose id is supplied, without an ownership check", true),
                ("Trusting a userId sent in the query string to identify the caller", true),
                ("Relying on hiding a UI button to prevent an action", true),
                ("Rendering user input into HTML without encoding", false)).Worth(3),
            Choice(
                "Server-side request forgery is best mitigated by:",
                ("An allow-list of destinations, validated after redirects", true),
                ("A deny-list of internal IP ranges", false),
                ("Encoding the response before returning it", false),
                ("Requiring POST instead of GET", false)).Worth(2),
            Choice(
                "Zero trust means:",
                ("Network location confers no implicit trust; every request is authenticated and authorised",
                    true),
                ("No employee is granted administrative access", false),
                ("All traffic must traverse a VPN", false),
                ("Passwords are eliminated in favour of biometrics", false)).Worth(2),
            Multiple(
                "Which belong in the containment phase of an incident?",
                ("Revoking active sessions and rotating exposed credentials", true),
                ("Isolating affected hosts from the network", true),
                ("Preserving volatile evidence before making changes", true),
                ("Holding the blameless post-incident review", false)).Worth(3)));
}
