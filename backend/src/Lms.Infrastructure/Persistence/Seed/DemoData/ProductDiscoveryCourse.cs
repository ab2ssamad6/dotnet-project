using Lms.Domain.Entities;
using Lms.Domain.Enums;

using static Lms.Infrastructure.Persistence.Seed.DemoData.DemoContent;

namespace Lms.Infrastructure.Persistence.Seed.DemoData;

public static class ProductDiscoveryCourse
{
    public const string Title = "Product Discovery and UX Research";

    public static Training Create(Category category, Trainer trainer) => NewTraining(
        Title,
        "Stop shipping features nobody asked for. This course covers the discovery habits that separate teams "
            + "who guess from teams who know: framing a problem before proposing a solution, interviewing users "
            + "without leading them, turning messy notes into decisions, testing prototypes with five people, "
            + "and measuring whether the thing you shipped actually changed behaviour.",
        DifficultyLevel.Beginner,
        category,
        trainer,
        FramingTheProblem(),
        TalkingToUsers(),
        Synthesis(),
        PrototypesAndTesting(),
        MeasuringOutcomes(),
        FinalAssessment());

    private static Module FramingTheProblem() => NewModule(
        "Framing the Problem Before the Solution",
        "Outcomes over outputs, assumption mapping, and writing a problem statement a team can act on.",
        70,
        aiAvatarEnabled: true,
        NewLesson(
            "Outputs, outcomes and the feature factory",
            """
            A feature factory measures itself by what it ships. Roadmaps list features, standups report
            progress towards release, and success is declared on launch day. The uncomfortable question -
            whether anything about a customer's behaviour changed - is never asked, because nobody agreed in
            advance what should change.

            Discovery replaces outputs with outcomes. An output is "a saved-search feature". An outcome is
            "returning users find a relevant training within two minutes instead of five". The second is
            falsifiable: it names who, what changes for them, and how you would know. It also leaves the
            solution open, which is the point - saved searches are one of perhaps six ways to get there, and
            they may not be the cheapest.

            The reframe is simple to say and hard to hold. When a stakeholder asks for a feature, the useful
            response is not yes or no but "what would be different for the user if we built it?" That question
            surfaces the outcome behind the request. Sometimes the outcome is real and the requested solution
            is poor; sometimes there is no outcome at all, only a competitor screenshot.

            Discovery is not a phase that ends before delivery starts. Teams that treat it as continuous - a
            few customer conversations every week, running alongside shipping - keep a live understanding of
            the problem instead of a snapshot that expires.
            """),
        NewLesson(
            "Assumption mapping and the riskiest question",
            """
            Every proposed feature rests on a stack of beliefs. Discovery is the practice of finding which of
            them, if wrong, would sink the idea - and testing that one first.

            List them in four categories. Value: do people want this enough to change what they do today?
            Usability: can they figure out how to use it? Feasibility: can we build and operate it? Viability:
            does it work for the business, legally and commercially?

            Then plot each assumption on two axes: how important it is, and how much evidence you actually
            have. Ignore the well-evidenced ones and the trivial ones. The top-right quadrant - critical and
            unevidenced - is your research agenda, usually two or three items, not twenty.

            Value assumptions are wrong most often and are tested least, because they are the uncomfortable
            ones. "Users want a dashboard" is a value assumption dressed as a requirement. The test is not a
            survey asking whether a dashboard would be useful - people say yes to everything hypothetical - but
            evidence of the underlying behaviour: are they exporting data into spreadsheets today, and what do
            they do with it once it is there?

            Write the outcome down before you test anything. A problem statement that names the user, the
            situation, the current workaround and the measurable change you expect is the artefact the whole
            team can argue with. Vague framing produces vague research, and vague research confirms whatever
            the loudest person already believed.
            """),
        NewExercise(
            "Write a problem statement and map its assumptions",
            """
            Take a feature currently on your roadmap:

            1. Write it as an outcome: which users, in what situation, and what measurably changes for them.
            2. List every assumption behind it under value, usability, feasibility and viability.
            3. Plot each on importance against existing evidence.
            4. Pick the single riskiest assumption and describe the cheapest test that could disprove it.
            5. Write the result that would make you abandon the feature.
            """,
            """
            An outcome statement with a named user and a measurable change, at least eight assumptions
            categorised and plotted, one identified riskiest assumption with a test that could realistically be
            run this week, and a stated kill criterion agreed before the test runs.
            """),
        NewQuiz(
            "Framing check",
            Choice(
                "Which of these is an outcome rather than an output?",
                ("Returning users find a relevant training in under two minutes", true),
                ("A saved-search feature on the catalog page", false),
                ("A redesigned navigation bar", false),
                ("Three new API endpoints", false)),
            Choice(
                "Which quadrant of an assumption map deserves research first?",
                ("High importance, low evidence", true),
                ("High importance, high evidence", false),
                ("Low importance, low evidence", false),
                ("Low importance, high evidence", false)),
            TrueFalse(
                "Discovery is a phase that should be completed before delivery begins.",
                false),
            Multiple(
                "Which belong in a usable problem statement?",
                ("The specific user or segment affected", true),
                ("The situation in which the problem occurs", true),
                ("The measurable change you expect", true),
                ("The technical design of the proposed solution", false))));

    private static Module TalkingToUsers() => NewModule(
        "Interviewing Without Leading",
        "Asking about past behaviour instead of future intentions, and running interviews that produce evidence.",
        75,
        aiAvatarEnabled: true,
        NewLesson(
            "Ask about the past, not the future",
            """
            People are unreliable narrators of their own future behaviour and reliable reporters of their
            recent past. That single asymmetry determines how a research interview should be written.

            "Would you use a feature that recommends trainings?" invites a polite yes that predicts nothing.
            "Tell me about the last time you looked for a training - what did you do first?" produces a story
            with timestamps, tools, workarounds and frustrations. Stories contain evidence; hypotheticals
            contain agreement.

            Three habits do most of the work. Anchor to a specific recent instance, because generalities smooth
            away the interesting parts - the answer to "usually" is fiction. Ask how they solve it today, since
            the current workaround reveals both the real need and the bar your solution must clear.

            And follow the emotion: when someone sighs, laughs, or says "obviously", something is worth
            digging into. "You said that was annoying - what happened exactly?" is the highest-yield question
            in research.

            Silence is a technique. After an answer, wait. The second thing people say is usually more honest
            than the first, and interviewers fill the gap far too quickly.
            """),
        NewLesson(
            "Running the session and staying honest",
            """
            Recruit for behaviour, not demographics. "People who searched the catalog in the last two weeks"
            is a useful screen; "product managers aged 30 to 45" usually is not. Five to eight participants per
            segment is enough for qualitative discovery - you are looking for patterns in reasoning, not
            statistical significance, and the same themes start repeating quickly.

            Structure the session loosely: five minutes of context, twenty-five on stories about the problem
            area, ten on any artefact or prototype you want reactions to, five to close. Bring a topic guide,
            not a script, and be willing to abandon it when the participant says something more interesting
            than your next question.

            Record with consent, and bring a notetaker so the interviewer can listen instead of type. Capture
            verbatim quotes - paraphrases quietly convert what the user said into what you expected to hear.

            Watch for the three ways interviews go wrong. Leading questions supply the answer inside the
            question. Pitching turns the session into a sales call, after which the participant will agree with
            everything to be pleasant. And confirmation bias in note-taking records the four quotes supporting
            the plan and none of the two that undermine it. A useful discipline: at the end of each session,
            write down the strongest thing you heard against your current idea. If you cannot find one across
            several sessions, you are pitching rather than listening.
            """),
        NewExercise(
            "Run three behavioural interviews",
            """
            Recruit three people who performed a relevant task recently:

            1. Write a topic guide with no hypothetical questions and no mention of your proposed solution.
            2. Open each session with a request for a specific recent story.
            3. Follow at least two emotional signals with "what happened exactly?"
            4. Record verbatim quotes, including everything that contradicts your idea.
            5. After each session, write the single strongest piece of counter-evidence you heard.
            """,
            """
            Three sets of notes dominated by concrete past events rather than opinions about the future, at
            least one documented workaround you did not know about, and three pieces of counter-evidence that
            genuinely challenge the plan.
            """),
        NewQuiz(
            "Interviewing check",
            Choice(
                "Which question produces the most reliable evidence?",
                ("Walk me through the last time you searched for a training", true),
                ("Would you use a recommendation feature?", false),
                ("How much would you pay for this?", false),
                ("Do you think other people would find this useful?", false)),
            Choice(
                "Why does the current workaround matter so much?",
                ("It reveals the real need and the bar a solution must clear", true),
                ("It proves users are technically capable", false),
                ("It sets the price point", false),
                ("It removes the need to prototype", false)),
            TrueFalse(
                "Five to eight participants per segment is usually enough for qualitative discovery.",
                true),
            Multiple(
                "Which behaviours undermine an interview?",
                ("Describing your proposed solution before asking about the problem", true),
                ("Rephrasing a participant's words to match your hypothesis in the notes", true),
                ("Sitting in silence for a few seconds after an answer", false),
                ("Asking whether they would buy a feature that does not exist yet", true))));

    private static Module Synthesis() => NewModule(
        "From Notes to Decisions",
        "Affinity mapping, separating observation from interpretation, and turning insight into an opportunity backlog.",
        70,
        aiAvatarEnabled: false,
        NewLesson(
            "Observation, interpretation, opportunity",
            """
            Synthesis fails when the three layers get mixed. Keep them explicit.

            An observation is what happened: "she opened a spreadsheet, pasted six course titles into it, and
            added a column for cost". An interpretation is what you think it means: "she needs to compare
            options side by side before requesting approval". An opportunity is what the team could act on:
            "make cost and duration comparable without leaving the catalog".

            Only the first is fact. Interpretations are hypotheses, and multiple people watching the same
            session will produce different ones - which is exactly why the interpretation step should be done
            together, out loud, with the observation visible. Teams that jump straight from a session to
            "so we should build a comparison table" have skipped the layer where they could have been wrong.

            The practical discipline is to write observations on their own, in the participant's language,
            before anyone proposes what to do about them. Sorting them into themes then produces patterns
            nobody arrived with. Affinity mapping is just this: one observation per note, group by similarity,
            name each group only after it forms. Naming first turns the exercise into filing evidence under
            conclusions you already held.
            """),
        NewLesson(
            "Opportunity trees and choosing what not to do",
            """
            Discovery generates far more opportunities than a team can pursue. An opportunity solution tree is
            the simplest structure for keeping that mess navigable: the outcome at the root, opportunities
            beneath it, candidate solutions beneath each opportunity, and tests beneath those.

            Two rules make it work. Opportunities are phrased as user needs, not features, so alternatives
            remain visible - "I cannot tell which course fits my level" rather than "add a difficulty filter".
            And the tree stays connected to one outcome; opportunities that do not serve it belong to a
            different problem, however interesting they are.

            Choosing between opportunities is a judgement call, but it can be made in the open. Compare how
            widespread the need is, how painful it is when it occurs, how much evidence supports it, and what
            it would cost to address. Most teams find that stating those four factors is more valuable than
            whatever scoring formula they wrap around them - the number rarely survives scrutiny, but the
            comparison does.

            Then write down what you are not doing and why. An explicit "not now, because the evidence is thin"
            prevents the same idea returning every month as a fresh proposal, and it makes the reasoning
            reviewable when evidence changes.
            """),
        NewExercise(
            "Synthesise a round of research",
            """
            Using notes from at least three sessions:

            1. Write every observation on its own note, in the participant's words.
            2. Group them by similarity without pre-naming the groups; name each group afterwards.
            3. For the three largest groups, write the interpretation separately from the observation.
            4. Build an opportunity tree with your outcome at the root and opportunities as user needs.
            5. Pick one opportunity to pursue and record why the others are waiting.
            """,
            """
            An affinity map whose group names emerged from the notes, interpretations clearly separated from
            observations, an opportunity tree phrased in needs rather than features, and an explicit written
            reason for each opportunity you are not pursuing yet.
            """),
        NewQuiz(
            "Synthesis check",
            Choice(
                "Which statement is an observation?",
                ("She pasted six course titles into a spreadsheet and added a cost column", true),
                ("She needs a comparison view", false),
                ("Users find the catalog hard to compare", false),
                ("We should build a side-by-side table", false)),
            Choice(
                "Why are opportunities phrased as needs rather than features?",
                ("It keeps alternative solutions visible instead of locking in one design", true),
                ("Needs are quicker to estimate", false),
                ("Features cannot be prioritised", false),
                ("It is required by the tree format", false)),
            TrueFalse(
                "Affinity groups should be named before the notes are sorted into them.",
                false),
            Multiple(
                "Which factors help compare opportunities honestly?",
                ("How many users encounter the need", true),
                ("How severe the pain is when it occurs", true),
                ("How much evidence supports it", true),
                ("How enthusiastic the loudest stakeholder is", false))));

    private static Module PrototypesAndTesting() => NewModule(
        "Prototypes and Usability Testing",
        "Choosing prototype fidelity, writing realistic tasks, and finding most problems with five participants.",
        75,
        aiAvatarEnabled: false,
        NewLesson(
            "Fidelity is a question of what you are testing",
            """
            A prototype is an instrument for answering one question, and its fidelity should be the lowest that
            answers it.

            Testing whether people understand a concept? A sketch or a written description is enough, and it
            invites criticism that a polished screen suppresses. Testing whether they can complete a flow? A
            clickable wireframe with real labels. Testing whether the interaction feels right - drag, timing,
            responsiveness? Now you need something close to real, because the feel is the question.

            Fidelity has a social cost. The more finished a prototype looks, the more participants comment on
            colours and the less they question the structure, and the more the team feels committed to it.
            Rough artefacts get honest reactions.

            Content matters more than pixels. Realistic labels, plausible data and real course names change
            behaviour; lorem ipsum makes a screen untestable because participants cannot tell what anything is.
            Two prototypes with identical layout and different copy routinely produce different success rates -
            the words are the interface.
            """),
        NewLesson(
            "Tasks, not demos",
            """
            A usability test is a set of tasks a participant attempts, with the facilitator mostly silent. It is
            not a walkthrough, and the difference decides whether you learn anything.

            Write tasks as goals with context and no interface vocabulary. "You want to improve your SQL before
            a project starts next month - find something suitable and sign up" is a task. "Click Catalog, then
            use the difficulty filter" is a script that tests nothing but the participant's ability to follow
            instructions.

            Facilitate lightly. Ask them to think aloud, then stay quiet. When they ask "what should I do
            here?" the answer is "what would you do if I weren't here?" Resist explaining, because in
            production nobody will be sitting next to them. Note where they hesitate, what they misread, and
            the gap between what they say they will do and what they then click.

            Five participants per round finds the large majority of usability problems - the classic finding is
            around 85 per cent - because the same obstacles recur quickly. It is far better to run three rounds
            of five with fixes between them than one round of fifteen that produces a report nobody acts on.

            Score by task, not by opinion. Did they complete it unaided, complete it with difficulty, or fail?
            Then fix the failures in order of severity and retest. "They liked it" is not a result.
            """),
        NewExercise(
            "Run a five-participant usability round",
            """
            Take any real flow, such as finding and enrolling in a training:

            1. Build the lowest-fidelity prototype that can answer your question, with realistic content.
            2. Write three goal-based tasks with no interface vocabulary.
            3. Test with five participants, thinking aloud, with minimal facilitation.
            4. Score each task as completed unaided, completed with difficulty, or failed.
            5. Fix the two most severe problems and retest with three more participants.
            """,
            """
            A task-by-task results table across both rounds, at least two specific problems identified with the
            exact point of failure, and measurable improvement on the retest rather than a general impression
            that it went better.
            """),
        NewQuiz(
            "Testing check",
            Choice(
                "Which is a well-written usability task?",
                ("You need to improve your SQL before next month - find something suitable and sign up", true),
                ("Click on Catalog and then use the difficulty filter", false),
                ("Tell me what you think of this screen", false),
                ("Would you say this design is intuitive?", false)),
            Choice(
                "Roughly how many usability problems does a round of five participants typically surface?",
                ("The large majority, around 85 per cent", true),
                ("Fewer than a quarter", false),
                ("Almost all of them, making further rounds unnecessary", false),
                ("It cannot be estimated at all", false)),
            TrueFalse(
                "A higher-fidelity prototype tends to attract comments on visual detail rather than structure.",
                true),
            Multiple(
                "Which facilitation habits keep a session valid?",
                ("Asking the participant to think aloud, then staying quiet", true),
                ("Answering 'what would you do if I weren't here?' when asked for help", true),
                ("Explaining the intended flow when they hesitate", false),
                ("Recording where they hesitate and what they misread", true))));

    private static Module MeasuringOutcomes() => NewModule(
        "Measuring Whether It Worked",
        "Choosing metrics that resist gaming, designing honest experiments, and closing the discovery loop.",
        70,
        aiAvatarEnabled: false,
        NewLesson(
            "Metrics that mean something",
            """
            Pick metrics before you build, and pick ones that would look bad if the feature failed. A metric
            chosen after launch is chosen to justify the launch.

            Distinguish the layers. A goal metric is the outcome you care about - learners completing a
            training. A driver metric is something you can move that plausibly leads to it - learners finishing
            the first module in week one. A guardrail metric is what must not get worse while you push the
            driver - support tickets, refund rate, time to first meaningful action.

            Guardrails are the part teams skip and regret. Almost any engagement number can be raised by
            annoying people: notifications lift return visits and raise unsubscribes; a mandatory step lifts
            profile completion and lowers signup completion. Without a guardrail the dashboard looks like a win.

            Prefer rates and cohorts to totals. "Enrollments" rises simply because time passes and the user
            base grows; "share of new users who enroll within seven days" is comparable across weeks. And beware
            vanity metrics - page views, registered accounts, feature clicks - that move without anything
            improving for anyone.
            """),
        NewLesson(
            "Experiments, and knowing when not to run one",
            """
            A controlled experiment answers one question: did this change cause that difference? It needs a
            control group, random assignment, a metric decided in advance, and a sample large enough to detect
            the effect you care about.

            Decide the sample size and the duration before starting, and stop at the planned point. Peeking at
            a running test and stopping when it looks significant manufactures false positives; running until
            something turns green does the same thing more slowly. Run at least a full week to cover the weekly
            cycle, since weekday and weekend behaviour differ.

            Many teams cannot run valid experiments, and pretending otherwise is worse than not testing. With
            low traffic, a two-week A/B test on a small effect will never reach significance. The honest
            alternatives are still evidence: a staged rollout with close monitoring, a before-and-after
            comparison with an explicit note about what else changed, or a qualitative round with ten users
            which - for usability problems - is more informative than an underpowered split test anyway.

            Close the loop either way. Write down what you expected, what happened, and what you now believe.
            A discovery practice without that final step accumulates activity rather than knowledge, and the
            same debates return every quarter.
            """),
        NewExercise(
            "Define the measurement plan for one feature",
            """
            Take a feature about to be built:

            1. Write one goal metric, one driver metric and two guardrail metrics.
            2. State the current baseline for each and the change that would count as success.
            3. Decide whether a controlled experiment is realistic at your traffic, and justify it.
            4. If not, design the strongest alternative evidence available to you.
            5. Write the decision rule: what result leads to keeping, iterating on, or removing the feature.
            """,
            """
            A one-page plan with baselines, a success threshold set before launch, guardrails that could
            genuinely fail, and an explicit decision rule agreed by the team in advance rather than negotiated
            after the numbers arrive.
            """),
        NewQuiz(
            "Measurement check",
            Choice(
                "What is a guardrail metric for?",
                ("Catching harm caused while pushing the primary metric", true),
                ("Replacing the goal metric when it does not move", false),
                ("Measuring engineering velocity", false),
                ("Tracking how many users saw the feature", false)),
            Choice(
                "Stopping an experiment as soon as the result looks significant:",
                ("Inflates false positives and invalidates the result", true),
                ("Is good practice because it saves traffic", false),
                ("Has no effect if the sample is random", false),
                ("Is required when a guardrail moves", false)),
            TrueFalse(
                "With low traffic, a qualitative round with ten users can be better evidence than an underpowered A/B test.",
                true),
            Multiple(
                "Which are better measurement choices?",
                ("A rate within a cohort rather than a cumulative total", true),
                ("Success thresholds agreed before launch", true),
                ("Metrics selected after seeing the launch dashboard", false),
                ("A written decision rule covering keep, iterate or remove", true))));

    private static Module FinalAssessment() => NewModule(
        "Final Assessment",
        "A timed exam covering framing, interviewing, synthesis, usability testing and measurement.",
        45,
        aiAvatarEnabled: false,
        NewExam(
            "Product Discovery and UX Research - Final Exam",
            Choice(
                "Which statement describes an outcome?",
                ("New learners complete their first module within seven days", true),
                ("We ship a recommendations carousel", false),
                ("The catalog page is redesigned", false),
                ("Two new filters are added", false)).Worth(2),
            Choice(
                "Assumption mapping tells you to test first:",
                ("The assumption that is critical and has the least evidence", true),
                ("The assumption that is cheapest to test", false),
                ("The technical feasibility assumption, always", false),
                ("Whichever assumption the stakeholder names", false)).Worth(2),
            Choice(
                "The most reliable interview question is:",
                ("Tell me about the last time you did this - what happened?", true),
                ("Would you use this if we built it?", false),
                ("How often do you usually do this?", false),
                ("Do you think this is a good idea?", false)).Worth(2),
            Multiple(
                "Which are true of synthesis?",
                ("Observations should be recorded separately from interpretations", true),
                ("Affinity groups are named after the notes are grouped", true),
                ("Opportunities are phrased as user needs", true),
                ("Counter-evidence can be left out if the pattern is clear", false)).Worth(3),
            Choice(
                "Prototype fidelity should be chosen based on:",
                ("The question the prototype needs to answer", true),
                ("How much time is left in the sprint", false),
                ("The seniority of the audience", false),
                ("The design system's component coverage", false)).Worth(2),
            TrueFalse(
                "A usability task should avoid naming the interface elements the participant is expected to use.",
                true).Worth(2),
            Choice(
                "A team cannot reach statistical significance at its traffic level. The honest response is:",
                ("Use staged rollout, close monitoring and qualitative evidence, and say so", true),
                ("Run the test anyway and report the direction of the change as a result", false),
                ("Extend the test until the numbers turn green", false),
                ("Skip measurement entirely", false)).Worth(2),
            Multiple(
                "Which belong in a measurement plan written before launch?",
                ("A goal metric and at least one driver metric", true),
                ("Guardrail metrics that could realistically fail", true),
                ("A success threshold and a decision rule", true),
                ("A list of metrics that are already trending upwards", false)).Worth(3)));
}
