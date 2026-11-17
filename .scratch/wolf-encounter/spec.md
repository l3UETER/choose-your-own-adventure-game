# Western wolf encounter

Status: ready-for-agent
Workflow: resolved

Part 4 Category A. Gameplay design and acceptance checklist approved after final shared-understanding review.

Implementation: completed through tickets 01, 02, and 03 after the user lifted each implementation hold. The user confirmed the manual Category A playthrough: the wolf encounter, one-time gold reward, and both merchant upgrades worked.

## Confirmed decisions

- Add the wolf encounter on the unused west route, accepting `w` and `west`.
- Winning or retreating returns the player to the travel menu.
- Preserve remaining player health and equipment when returning to travel.
- Defeat ends the adventure.
- Arriving west shows the wolf and offers Attack or Retreat, matching the dragon's initial interaction. Retreat before combat returns to travel without damage.
- Wolf combat uses the dragon's combat rules: player attacks first; strength determines hits and agility determines defense; a surviving wolf retaliates before the next Attack/Retreat choice. Magical armor and weapon enchantment apply normally.
- Roll wolf strength, agility, and health independently from 1–15. The dragon retains its higher 1–20 stat range and boss role. Individual wolf rolls can still exceed individual dragon rolls.
- A successful, undefended wolf bite rolls 1–6 damage before magical armor reduces it, with a minimum of zero damage.
- Each adventure has one persistent wolf. Its rolled statistics and remaining health persist after retreat. Once defeated, the western route stays clear for that adventure.
- Defeating the wolf awards 10 gold exactly once per adventure. Retreat and repeat visits award nothing. This intentionally expands the earlier merchant specification's no-additional-gold-source constraint: 20 starting gold plus the reward permits buying both 15-gold upgrades.
- Invalid combat input repeats the Attack/Retreat prompt without attacks or health changes in both wolf and dragon encounters. Correct the current dragon loop's behavior of advancing after invalid input.
- End-of-input at either encounter's initial or in-combat Attack/Retreat prompt counts as Retreat. Wolf retreat returns to travel (where closed input exits); dragon retreat retains its adventure-ending outcome.
- Before the initial Attack/Retreat choice, show a short wolf introduction and its name, strength, agility, current health, and bite damage range, matching the dragon encounter's information. After its defeat, show that the western route is clear instead of offering combat again.
- Use the name "Wolf" (localized for display) and animal narration for biting, dodging, and taking damage, without spoken dragon taunts.
- Localize new travel, wolf, combat, reward, and cleared-route text in English, Spanish, French, and Italian using the existing JSON mechanism and matching placeholders.

## Existing behavior reviewed before implementation

- Part 3 merchant is implemented on the south route, including magical armor and weapon enchantment purchases.
- North leads to the dragon and concludes the adventure on victory, defeat, or retreat.
- `e` already means exit; west is currently rejected.
- Combat and player attacks currently depend directly on Dragon. The player attacks first; a surviving dragon retaliates, then the player can continue or retreat.
- Existing dice injection supports deterministic combat and journey tests.
- The accepted localization ADR requires matching message coverage and placeholders across English, Spanish, French, and Italian.

## Approval

The user confirmed the complete plan and acceptance checklist, authorized all three tickets, and completed the manual playthrough. The user has now authorized pushing the verified Category A changes to their fork.

## Accepted validation checklist

Use the existing controlled-dice and console-testing approach for deterministic behavior checks.

- West accepts `w` and `west`; north, south, and exit navigation remain available.
- Initial retreat returns to travel without damage. Victory and in-combat retreat preserve player health and equipment and return to travel; defeat ends the adventure.
- Wolf strength, agility, and health are independently generated within 1–15. Bite damage is within 1–6 before armor. Verify player-first rounds, hit and defense checks, and both merchant equipment effects.
- Retreat preserves wolf rolls and remaining health; later visits show current health. Victory leaves the route clear and prevents further fights against that wolf. A new adventure has fresh encounter state.
- Victory grants exactly 10 gold once. Retreat and cleared-route visits grant none. Verify the reward enables purchase of both upgrades across merchant visits.
- Invalid input at either encounter's initial and in-combat prompts does not advance combat or change health. End-of-input acts as retreat at each prompt, without looping indefinitely.
- Verify wolf-specific introduction, stats, narration, reward, and cleared-route output; no dragon taunts appear in wolf combat.
- New messages are nonempty in all four languages and preserve English placeholder counts and indices.
- Run the existing test suite for regressions, retaining northern adventure-ending behavior.
