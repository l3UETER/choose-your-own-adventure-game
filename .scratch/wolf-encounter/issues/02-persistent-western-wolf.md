# 02: Fight or retreat from a persistent western wolf

Status: ready-for-agent
Workflow: resolved

**What to build:** Travel west to meet a Wolf, inspect its statistics, and choose Attack or Retreat. Surviving returns the player to travel with remaining health and equipment. Retreat leaves the same wounded wolf for later; victory permanently clears the route for this adventure. Defeat ends the adventure.

**Blocked by:** None (can start immediately).

**Execution hold:** Lifted by the user for Ticket 02 only. Ticket 03 remains on hold.

## Source and acceptance mapping

[Approved specification: Accepted validation checklist](../spec.md#accepted-validation-checklist), items 1–4 and 6–9. Item numbers refer to the bullets in that checklist, in order. Reward behavior from item 5 and reward narration from item 7 are delivered by ticket 03.

- [x] Accept `w` and `west` in travel input and display the west option. Preserve north, south, and exit commands, including `e` for exit; align path validation and the actual travel prompt.
- [x] On an uncleared route, show a short introduction, the localized name "Wolf", strength, agility, current health, and bite damage range before offering Attack or Retreat.
- [x] Roll strength, agility, and health independently within 1–15, using the existing controllable dice approach. The dragon retains its 1–20 statistics and existing damage range.
- [x] Use the existing combat rules: the player attacks first, a surviving wolf retaliates, then offer the next Attack/Retreat choice. Strength determines hits and agility determines defense using the same checks as dragon combat.
- [x] A successful, undefended bite rolls 1–6 damage. Magical armor reduces it by 3 with a minimum of zero; weapon enchantment adds its normal bonus to successful, undefended player hits. Missed and defended attacks deal no damage.
- [x] Initial retreat returns to travel without damage. In-combat retreat and victory return to travel, preserving current player health, gold, weapon, and equipment; defeat ends the adventure. Do not use dragon victory or retreat endings for wolf survival.
- [x] Keep one wolf per adventure: rolled statistics and remaining health survive retreat and repeat visits. Defeat of the wolf produces a clear-route message on later visits without offering another fight. A new adventure has fresh wolf state.
- [x] Invalid initial or in-combat wolf input repeats the prompt without attacks, health changes, or consuming combat rolls. Closed input acts as Retreat; closed input at the following travel prompt exits without looping.
- [x] Use animal narration for biting, dodging, taking damage, and defeat; never emit dragon names, spoken taunts, or dragon-specific headers in wolf combat.
- [x] Deliver all new travel, introduction, stats, combat, and clear-route text in English, Spanish, French, and Italian. Check nonempty messages and English-equivalent placeholder counts and indices.
- [x] Add deterministic combat and journey checks for navigation, stat and damage bounds, turn order, hit/defense checks, armor and enchantment effects, initial and mid-combat retreat, victory, defeat, repeated visits, fresh adventure state, invalid input, and closed input.
- [x] Run the existing test suite, updating the previous expectation that west is invalid. Preserve the merchant and northern adventure-ending behavior.

## Scope and integration

This slice delivers a playable encounter without the victory reward, which is added in ticket 03. Avoid awarding gold on retreat or clear-route visits. Any combat adaptation needed for a non-dragon opponent belongs with this behavior and its tests; no broad standalone refactor is required by this ticket.

Ticket 01 independently corrects dragon input. If these tickets touch shared combat handling, coordinate the changes and verify both encounters; that overlap is not a functional blocking dependency.

## Comments

Implemented using deterministic journey tests through Game.StartGame. The first test failed because west was rejected and no Wolf appeared, then passed after navigation and initial retreat were added. Subsequent red-green slices added combat, persistence, victory and cleared routes, defeat, invalid input, and animal narration. Closed-input checks pass at both prompts. Tests verify independent 1-15 stats, 1-6 bites, threshold defenses, armor and enchantment, preserved health and equipment, fresh adventure state, and four-language message coverage and placeholders.

Validation: 21 focused cases passed; full dotnet test passed 182 tests with zero failures or skips. Standards and spec reviews found no blocking issues. Removed duplicated test setup and explicitly covered enchantment across retreat and repeat encounters. Existing messages and maps are preserved except approved travel prompts. No gold reward was implemented; Ticket 03 was not started.
