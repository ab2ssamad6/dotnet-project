using Lms.Domain.Entities;
using Lms.Domain.Enums;

using static Lms.Infrastructure.Persistence.Seed.DemoData.DemoContent;

namespace Lms.Infrastructure.Persistence.Seed.DemoData;

public static class ModernReactCourse
{
    public const string Title = "Modern React: Interfaces That Scale";

    public static Training Create(Category category, Trainer trainer) => NewTraining(
        Title,
        "Build React applications that stay maintainable past the first thousand lines. You will work through "
            + "state that lives in the right place, effects that do not fire twice by accident, composition "
            + "patterns that replace prop drilling, rendering performance measured rather than guessed, and "
            + "forms that are accessible and covered by tests users would recognise.",
        DifficultyLevel.Intermediate,
        category,
        trainer,
        ComponentsAndState(),
        EffectsAndData(),
        CompositionAndContext(),
        RenderingPerformance(),
        FormsAccessibilityTesting(),
        FinalAssessment());

    private static Module ComponentsAndState() => NewModule(
        "Components, Props and State That Behaves",
        "Rendering as a function of state, why updates are batched, and where a piece of state belongs.",
        75,
        aiAvatarEnabled: true,
        NewLesson(
            "Rendering is a function of state",
            """
            A React component is a function from props and state to a description of the UI. React calls it,
            compares the result with the previous description, and applies the smallest set of DOM operations
            that reconciles the two. You never write the DOM update; you write what the screen should look like
            for the current values.

            This is why mutating state in place does nothing. Pushing onto an array that lives in state leaves
            the reference unchanged, React's comparison sees the same object, and the screen stays stale. Every
            update must produce a new value: setItems([...items, item]), not items.push(item).

            State updates are also asynchronous and batched. Inside an event handler, several setState calls are
            collected and applied in one pass, so reading the state variable immediately after setting it gives
            you the old value - it belongs to the render that is still on screen. When the next value depends on
            the previous one, use the functional form: setCount(c => c + 1). Two increments written that way
            both apply; two written as setCount(count + 1) collapse into one.

            Keys deserve the same care. A key tells React which item in a list is which across renders. Using
            an array index as the key breaks the moment the list is reordered or filtered, because item
            identity silently shifts and component state follows the wrong row.
            """),
        NewLesson(
            "Where state belongs",
            """
            Most confusing React code is state in the wrong place. Four questions settle it.

            Is it derived? Then it is not state. A filtered list, a total, a validity flag - compute them
            during render from what you already have. Storing derived values creates two sources of truth that
            drift apart, and the bug always looks like "the total is one click behind".

            Who needs it? Keep state in the component that uses it. When two siblings need the same value, lift
            it to their nearest common parent and pass it down - but only as far as necessary. Hoisting
            everything to the top of the tree turns every keystroke into an application-wide render.

            Does the URL own it? Filters, tabs, pagination and the currently selected record usually belong in
            the query string. Then a refresh, a shared link and the back button all behave, and you delete the
            state entirely.

            Does the server own it? Data fetched from an API is a cache, not state. It has a loading status, an
            error status, a staleness question and possibly a background refresh. Modelling it as three
            useState calls is how you end up with race conditions; a data-fetching library or one reducer that
            owns the whole shape is a better fit.
            """),
        NewExercise(
            "Fix a component with too much state",
            """
            Take a list screen that keeps items, filteredItems, searchTerm, isEmpty and selectedId in state:

            1. Remove every value that can be derived and compute it during render instead.
            2. Move the search term and any active filter into the query string.
            3. Replace an index-based list key with a stable id and prove the difference by reordering the list.
            4. Convert a counter-style update to the functional form and show why it matters when called twice.
            5. List which state remains and justify each one in a sentence.
            """,
            """
            A component with markedly less state, filters that survive a page refresh and the back button, and
            a reorder that no longer moves per-row state to the wrong row.
            """),
        NewQuiz(
            "State fundamentals check",
            Choice(
                "Why does items.push(newItem) fail to update the screen?",
                ("The array reference is unchanged, so React sees no new value", true),
                ("push is not allowed inside components", false),
                ("Arrays cannot be stored in state", false),
                ("The component is missing a key prop", false)),
            Choice(
                "Which update form is correct when the next value depends on the previous one?",
                ("setCount(c => c + 1)", true),
                ("setCount(count + 1)", false),
                ("count = count + 1", false),
                ("setCount(count++)", false)),
            TrueFalse(
                "A value that can be computed from existing props or state should not be stored in state.",
                true),
            Multiple(
                "Which pieces of state are usually better kept in the URL?",
                ("The active filter", true),
                ("The current page number", true),
                ("The text of a half-typed comment", false),
                ("The selected record id on a master-detail screen", true))));

    private static Module EffectsAndData() => NewModule(
        "Effects, Data Fetching and Async State",
        "What effects are actually for, cleaning them up, and handling loading, errors and races honestly.",
        80,
        aiAvatarEnabled: true,
        NewLesson(
            "Effects are for synchronising with the outside world",
            """
            useEffect exists to synchronise a component with something React does not control: a subscription,
            a timer, a browser API, an analytics call, a network request. It is not a lifecycle hook and it is
            not where you compute things.

            Two habits cause most effect bugs. The first is using an effect to derive state - an effect that
            watches items and sets total. That renders once with the wrong value, then again with the right
            one, and can loop. Compute it during render instead. The second is using an effect to respond to a
            user action; the click handler already knows what happened, so do the work there.

            The dependency array is a claim about what the effect reads. Leave a value out and the effect keeps
            a stale copy of it forever. Include a function or object recreated on every render and the effect
            runs on every render. The honest fixes are to move the value inside the effect, wrap the function in
            useCallback, or hoist it out of the component entirely - never to delete a dependency to silence the
            linter.

            Every effect that starts something must return a cleanup that stops it. Timers cleared, listeners
            removed, sockets closed, requests aborted. In development, Strict Mode deliberately mounts, unmounts
            and remounts components so that a missing cleanup fails loudly - a doubled request there is your
            code telling you about a leak, not React misbehaving.
            """),
        NewLesson(
            "Loading, errors and the race you have not noticed",
            """
            Fetching in an effect looks simple and hides a race. A user types "ab", then "abc"; two requests are
            in flight; the slower one for "ab" resolves last and overwrites the correct results. The screen now
            disagrees with the input, and it reproduces only on a bad connection.

            Two fixes, used together. Abort the previous request in the cleanup with an AbortController, and
            ignore any response whose input no longer matches the current one. The cleanup runs before the
            effect re-runs, which is exactly the moment the older request became irrelevant.

            Model async state as one value, not three booleans. isLoading, isError and data drifting apart is
            how "loading" and "error" end up on screen together. A single status - idle, loading, success,
            error - with the payload attached to the success case makes impossible combinations unrepresentable.

            Then design the three states as real UI. Loading should preserve layout rather than collapsing it -
            skeletons the shape of the content beat a centred spinner. Errors need a cause and an action, not
            just a red banner. Empty is a distinct state from loading and deserves its own copy and a next step.
            """),
        NewExercise(
            "Make a search screen race-proof",
            """
            Build or fix a debounced search that fetches results as the user types:

            1. Reproduce the race by throttling the network and typing quickly.
            2. Add an AbortController and abort the in-flight request from the effect cleanup.
            3. Collapse isLoading, isError and data into a single status value.
            4. Give loading, error and empty their own components, keeping layout stable across them.
            5. Confirm in Strict Mode that no duplicate subscription or timer survives a remount.
            """,
            """
            A search box that always displays results for the current query, no impossible loading-and-error
            combination, and clean remounts in Strict Mode with nothing left running.
            """),
        NewQuiz(
            "Effects check",
            Choice(
                "Which job genuinely needs useEffect?",
                ("Subscribing to a browser event and unsubscribing on unmount", true),
                ("Calculating a total from a list already in state", false),
                ("Reacting to a button click", false),
                ("Formatting a date for display", false)),
            Choice(
                "Why does Strict Mode run effects twice in development?",
                ("To surface effects that lack a correct cleanup", true),
                ("To benchmark render performance", false),
                ("Because state updates are batched", false),
                ("To pre-warm the production build", false)),
            TrueFalse(
                "Removing a value from the dependency array is an acceptable way to stop an effect re-running.",
                false),
            Multiple(
                "Which prevent a stale response from overwriting fresh results?",
                ("Aborting the previous request in the effect cleanup", true),
                ("Ignoring responses whose query no longer matches the current one", true),
                ("Increasing the debounce delay until it stops happening", false),
                ("Keying the cache entry by the query string", true))));

    private static Module CompositionAndContext() => NewModule(
        "Composition, Context and Reusable Logic",
        "Replacing prop drilling with composition, using context deliberately, and extracting custom hooks.",
        75,
        aiAvatarEnabled: false,
        NewLesson(
            "Composition before configuration",
            """
            The instinct when a component needs to vary is to add a prop. Repeat that ten times and you have a
            component with a dozen booleans, a body full of conditionals, and no safe way to change anything.

            Composition solves it. Instead of Card taking showHeader, headerIcon, headerAction and
            footerAlignment, let it take children and expose CardHeader and CardFooter. The caller assembles
            exactly the card it needs, the component keeps one job, and adding a new arrangement requires no
            change to Card at all.

            Passing elements as props is the same idea and is often overlooked as a fix for prop drilling. If a
            deep child needs data, the parent that owns the data can render the element and pass it down as a
            prop; the intermediate layers carry one opaque node instead of five values they never look at.

            When a family of components must share implicit state - tabs and their panels, a menu and its
            items - a small compound-component context is appropriate. The API stays declarative for the
            caller, and the coordination is invisible rather than threaded through props.
            """),
        NewLesson(
            "Context, and hooks that hide the plumbing",
            """
            Context is a transport for values that are genuinely global to a subtree: the signed-in user, the
            theme, the locale, a toast dispatcher. It is not a state manager, and it is not a performance tool.

            Its cost is bluntly simple: when a provider's value changes, every consumer re-renders. Put a
            frequently changing value in a context that wraps the application and you have built an
            application-wide render on every keystroke. Two mitigations matter. Split contexts by change
            frequency - a stable dispatch context beside a volatile data context - and memoise the provider
            value so a new object identity is not created on every parent render.

            Custom hooks are how the plumbing disappears. A hook is a function that calls other hooks; there is
            nothing more to it. useDisclosure, usePagedList, useClickOutside - each takes a pattern that
            appeared in four components and gives it one implementation, one name and one place to fix.

            Keep hooks honest. Return a stable, minimal API rather than fifteen values. Do not hide a network
            call behind a name that sounds synchronous. And remember the rule that makes them work at all: call
            hooks unconditionally, at the top level, in the same order on every render.
            """),
        NewExercise(
            "Refactor a prop-heavy component",
            """
            Find a component with more than six props and untangle it:

            1. Group the props into the shapes they actually describe.
            2. Convert the layout switches into composition with children and named sub-components.
            3. Replace a value drilled through three or more layers with either an element prop or context.
            4. Extract one repeated stateful pattern into a custom hook used by at least two components.
            5. Measure re-renders before and after with the React DevTools profiler.
            """,
            """
            A component whose prop list has shrunk substantially, call sites that read as markup rather than
            configuration, one genuinely shared hook, and a profiler recording showing fewer components
            re-rendering per interaction.
            """),
        NewQuiz(
            "Composition check",
            Choice(
                "A component has grown a dozen boolean props. The best first move is:",
                ("Expose children and sub-components so callers compose the arrangement", true),
                ("Group the booleans into a single options object", false),
                ("Split it by copying it into two similar components", false),
                ("Move the booleans into context", false)),
            Choice(
                "What happens when a context provider's value changes?",
                ("Every consumer of that context re-renders", true),
                ("Only the nearest consumer re-renders", false),
                ("Nothing until the next effect runs", false),
                ("The whole tree remounts", false)),
            TrueFalse(
                "Hooks must be called unconditionally and in the same order on every render.",
                true),
            Multiple(
                "Which reduce unnecessary re-renders from context?",
                ("Splitting stable dispatch and volatile data into separate contexts", true),
                ("Memoising the provider value instead of building a new object each render", true),
                ("Wrapping the entire application in one context holding all state", false),
                ("Moving fast-changing state closer to the components that use it", true))));

    private static Module RenderingPerformance() => NewModule(
        "Rendering Performance You Can Measure",
        "Finding real bottlenecks with the profiler, memoising with intent, and handling large lists.",
        70,
        aiAvatarEnabled: false,
        NewLesson(
            "Measure first, memoise second",
            """
            React performance work goes wrong when it starts with useMemo. Memoisation is not free: it costs a
            comparison on every render, holds references alive, and adds a dependency array that will eventually
            be wrong. Applied everywhere, it makes an application slower and harder to reason about.

            Start with the React DevTools profiler. Record an interaction that feels slow and read two things:
            which components rendered, and how long the commit took. The answer is usually one of a small set of
            causes - a parent re-rendering the whole subtree because state sits too high, a context carrying a
            fast-changing value, an unstable prop such as an inline object or arrow function defeating an
            already-memoised child, or a list rendering hundreds of rows at once.

            Fix the cause before reaching for a wrapper. Moving state down so a keystroke only re-renders one
            input is a structural fix that keeps paying off; wrapping the tree in React.memo to absorb that
            keystroke is a patch that hides it.

            When you do memoise, do it with a target. React.memo on a component that is expensive and receives
            stable props. useMemo for a genuinely costly computation or for a value used as a dependency
            elsewhere. useCallback for a function passed to a memoised child or used in a dependency array.
            Then profile again and confirm the number moved.
            """),
        NewLesson(
            "Long lists and perceived speed",
            """
            The most common real bottleneck is a list. A thousand rows means a thousand components, their
            children, their event handlers and their DOM nodes - on every render.

            Paginate if the product allows it, because it is simpler than anything else and reduces the data
            you fetch as well. If the interaction requires a continuous list, virtualise: render only the rows
            in the viewport plus a small buffer, and translate a spacer to keep the scrollbar honest. The number
            of mounted rows becomes proportional to the window, not to the data.

            Keep the row cheap either way. Stable keys, memoised rows, handlers that do not change identity per
            render, and no expensive formatting inside the row body - hoist date and currency formatters, since
            constructing an Intl formatter per cell is a measurable cost at scale.

            Finally, remember that perceived performance is a separate axis. Skeletons that match the final
            layout, optimistic updates that apply immediately and roll back on failure, and transitions that
            keep the old view interactive while the new one loads all make an application feel fast without
            making it faster. Users experience latency as jank and layout shift far more than as milliseconds.
            """),
        NewExercise(
            "Profile and fix one real bottleneck",
            """
            Pick the slowest screen in an application you have:

            1. Record the interaction in the profiler and note the commit duration and the components that rendered.
            2. Identify the cause: state too high, unstable props, a hot context, or list size.
            3. Apply the structural fix first and re-record.
            4. Only then add targeted memoisation, and re-record again to prove it helped.
            5. Add skeletons that preserve layout and compare how the screen feels.
            """,
            """
            Before and after profiler recordings with a real reduction in rendered components or commit time, a
            written cause, and at most a couple of memoisation calls that each have measured justification.
            """),
        NewQuiz(
            "Performance check",
            Choice(
                "What should come first when a screen feels slow?",
                ("A profiler recording that identifies which components render and why", true),
                ("Wrapping the top-level component in React.memo", false),
                ("Adding useMemo around every derived value", false),
                ("Switching to a lighter UI library", false)),
            Choice(
                "Why can React.memo fail to prevent a re-render?",
                ("An inline object or arrow function prop changes identity every render", true),
                ("Memoised components ignore state changes", false),
                ("It only works on class components", false),
                ("It requires a key prop to function", false)),
            TrueFalse(
                "Virtualisation keeps the number of mounted rows proportional to the viewport rather than the data.",
                true),
            Multiple(
                "Which improve perceived performance without changing raw speed?",
                ("Skeletons that match the final layout", true),
                ("Optimistic updates that roll back on failure", true),
                ("Keeping the previous view interactive while new data loads", true),
                ("Delaying the spinner until the request finishes", false))));

    private static Module FormsAccessibilityTesting() => NewModule(
        "Forms, Accessibility and Tests Worth Keeping",
        "Controlled inputs and validation, accessible semantics by default, and tests written the way users behave.",
        75,
        aiAvatarEnabled: false,
        NewLesson(
            "Forms and validation that respect the user",
            """
            A controlled input takes its value from state and reports changes through onChange; React owns the
            truth. It gives you formatting, conditional fields and cross-field rules, at the cost of a render
            per keystroke - fine for a login box, wasteful for a forty-field form, which is where uncontrolled
            inputs read on submit, or a form library that subscribes per field, earns its place.

            Validation is a timing problem more than a rules problem. Validating on every keystroke shouts at
            people while they are still typing their email address. The pattern users tolerate is: validate a
            field when it loses focus, validate everything on submit, and once a field is showing an error,
            re-validate it as they type so the message clears the moment it is fixed.

            Define the rules once. A schema shared by client and server keeps the two from disagreeing, and the
            server rules are the ones that matter - client validation is a convenience, never a control.

            Handle submission states explicitly. Disable the submit button while the request is in flight so a
            double click cannot create two records, keep the entered values on failure, and put server-side
            field errors back onto the fields they belong to rather than in a banner at the top.
            """),
        NewLesson(
            "Accessibility is mostly using the right element",
            """
            Most accessibility work in React is choosing semantic elements rather than adding ARIA. A button is
            a button: it is focusable, it fires on Enter and Space, it is announced as a button. A div with an
            onClick is none of those things, and the three attributes needed to fake it will be forgotten.

            Labels must be programmatically associated, not merely adjacent. htmlFor pointing at the input's id
            is what makes a screen reader announce the field, and it makes the label clickable for everyone
            else. Placeholder text is not a label - it disappears exactly when the user needs it.

            Errors need to be linked to their field with aria-describedby and marked with aria-invalid, and
            asynchronous messages - a toast, a validation summary, a search result count - belong in a live
            region, or they are silently invisible to anyone not looking at that part of the screen.

            Then check the three things that catch most defects: can you complete the whole flow with the
            keyboard alone, is the focus indicator always visible, and does focus move sensibly when a dialog
            opens and closes. A dialog that traps focus while open and returns it to the trigger on close is the
            single most valuable widget-level fix in most applications.
            """),
        NewExercise(
            "Harden and test a real form",
            """
            Take a form with at least five fields:

            1. Validate on blur, on submit, and re-validate a failed field as the user types.
            2. Share one schema between the client and the server contract.
            3. Associate every label, error and hint with its input using htmlFor and aria-describedby.
            4. Complete the entire flow with the keyboard only, and fix whatever you cannot reach.
            5. Write tests that query by label and role, cover the error path and the double-submit case.
            """,
            """
            A form that is fully keyboard operable, errors announced and linked to their fields, a submit that
            cannot fire twice, and tests that would still pass after the markup is restyled.
            """),
        NewQuiz(
            "Forms and accessibility check",
            Choice(
                "Which validation timing is least frustrating for users?",
                ("On blur and on submit, then live once a field is already showing an error", true),
                ("On every keystroke from the first character", false),
                ("Only on the server after submission", false),
                ("Only when the user asks for it", false)),
            Choice(
                "Why prefer a real button over a div with onClick?",
                ("It is focusable, keyboard operable and announced correctly by default", true),
                ("It renders faster", false),
                ("Divs cannot receive click handlers", false),
                ("It avoids a React warning", false)),
            TrueFalse(
                "Placeholder text is an acceptable replacement for a visible, associated label.",
                false),
            Multiple(
                "Which make component tests durable?",
                ("Querying by accessible role and label text", true),
                ("Asserting on what the user sees rather than internal state", true),
                ("Selecting elements by CSS class names", false),
                ("Covering the error path as well as the happy path", true))));

    private static Module FinalAssessment() => NewModule(
        "Final Assessment",
        "A timed exam covering state, effects, composition, rendering performance, forms and accessibility.",
        45,
        aiAvatarEnabled: false,
        NewExam(
            "Modern React: Interfaces That Scale - Final Exam",
            Choice(
                "A list rendered from state does not update after an item is added. The most likely cause is:",
                ("The array was mutated, so its reference never changed", true),
                ("The component is missing an effect", false),
                ("State updates are synchronous and ran too early", false),
                ("The list needs to be wrapped in React.memo", false)).Worth(2),
            Choice(
                "Which value should not be stored in state?",
                ("A total that can be computed from items already in state", true),
                ("Text the user is currently typing", false),
                ("Whether a dialog is open", false),
                ("The response of a completed upload", false)).Worth(2),
            Choice(
                "The correct place to abort a superseded fetch is:",
                ("The cleanup function returned by the effect", true),
                ("A finally block after the response is parsed", false),
                ("The component's unmount handler only", false),
                ("A global error boundary", false)).Worth(2),
            Multiple(
                "Which are legitimate uses of useEffect?",
                ("Subscribing to a browser API and cleaning up on unmount", true),
                ("Starting and clearing an interval timer", true),
                ("Deriving a filtered list from props", false),
                ("Synchronising component state with an external store", true)).Worth(3),
            Choice(
                "Placing fast-changing state in an application-wide context typically causes:",
                ("Every consumer of that context to re-render on each change", true),
                ("A hydration mismatch", false),
                ("Effects to run twice", false),
                ("Keys to be recalculated", false)).Worth(2),
            TrueFalse(
                "Memoisation should be applied after profiling identifies a specific bottleneck.",
                true).Worth(2),
            Choice(
                "The most reliable way to stop a double submit creating two records is:",
                ("Disable the submit control while the request is in flight, and de-duplicate server-side", true),
                ("Debounce the button by 300 milliseconds", false),
                ("Validate the form again inside onSubmit", false),
                ("Show a confirmation dialog", false)).Worth(2),
            Multiple(
                "Which are true of accessible forms?",
                ("Labels are associated with inputs via htmlFor and id", true),
                ("Error messages are linked with aria-describedby and marked with aria-invalid", true),
                ("A visible focus indicator can be removed if the design prefers it", false),
                ("The whole flow can be completed with the keyboard alone", true)).Worth(3)));
}
