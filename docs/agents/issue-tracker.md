# Issue tracker: Local Markdown

Issues and specs live as Markdown files under `.scratch/`.

## Conventions

- One feature per directory: `.scratch/<feature-slug>/`.
- Specs live at `.scratch/<feature-slug>/spec.md`.
- Implementation tickets live at
  `.scratch/<feature-slug>/issues/<NN>-<slug>.md`, numbered from `01`,
  with one file per ticket.
- Record triage state as a `Status:` line near the top of each ticket.
  Use the role strings in `triage-labels.md`.
- Append comments and conversation history under `## Comments`.
- Resolve bare issue numbers within the referenced feature directory.
  If the feature is ambiguous, ask which directory applies.

## Publishing and fetching

When a skill says "publish to the issue tracker", create the appropriate
spec or ticket file using the paths above, creating directories as needed.

When a skill says "fetch the relevant ticket", read the referenced file.

## Wayfinding operations

Used by `/wayfinder`.

- Map: `.scratch/<effort>/map.md`, containing Notes, Decisions-so-far,
  and Fog sections.
- Child ticket: `.scratch/<effort>/issues/<NN>-<slug>.md`.
- Type: record `research`, `prototype`, `grilling`, or `task` in a
  `Type:` line.
- Lifecycle: record `open`, `claimed`, or `resolved` in a `Workflow:` line,
  keeping `Status:` for triage roles.
- Blocking: record dependencies as `Blocked by: NN, NN`. A ticket is
  unblocked when every listed ticket has `Workflow: resolved`.
- Frontier: select the first ticket by number that is open and unblocked.
- Claim: set `Workflow: claimed` and save before starting work.
- Resolve: append the answer under `## Answer`, set `Workflow: resolved`,
  and append a brief finding and relative link to Decisions-so-far
  in the map.
