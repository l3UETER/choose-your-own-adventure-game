# 01: Handle invalid and closed input safely in the dragon encounter

Status: ready-for-agent
Workflow: resolved

**What to build:** At both dragon Attack/Retreat prompts, allow a player to correct invalid input without triggering attacks or changing health. Closed input behaves as Retreat and concludes the adventure normally.

**Blocked by:** None (can start immediately).

**Execution hold:** Lifted by the user for Ticket 01 only.

## Source and acceptance mapping

[Approved specification: Accepted validation checklist](../spec.md#accepted-validation-checklist), items 6 and 9. Item numbers refer to the bullets in that checklist, in order.

- [x] Invalid input at the initial dragon choice repeats the prompt without starting combat, consuming combat rolls, or changing health.
- [x] Invalid input after a combat round repeats the prompt without either participant attacking or changing health; a later valid Attack advances exactly one round.
- [x] Closed input at either prompt acts as Retreat and terminates without an infinite loop or extra attack.
- [x] Preserve the dragon's player-first rounds, hit and defense rules, equipment effects, and adventure-ending victory, defeat, and retreat outcomes.
- [x] Use deterministic dice and redirected console input/output to verify the initial and in-combat cases, including repeated invalid input followed by a valid choice.
- [x] Reuse existing localized messages where suitable; any new messages have nonempty English, Spanish, French, and Italian text and matching placeholder counts and indices (checklist item 8 if new messages are introduced).
- [x] Run the existing test suite for regressions.

## Scope

This is a complete player-visible input correction, not a requirement to redesign all combat. Keep any preparatory refactoring narrow and preserve existing public behavior beyond the approved input corrections. The western wolf encounter can be developed independently; this ticket does not require its existence.

## Comments

Implemented Ticket 01 only. The invalid-input regression failed on an unexpected combat roll before production changes, then passed with a prompt-only retry loop. Closed-input tests failed at both prompts before null was treated as Retreat. All five focused cases pass; full dotnet test passes 161 tests. Existing combat and equipment regressions pass. No new localized messages.

