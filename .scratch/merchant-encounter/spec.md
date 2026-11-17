# Southern merchant and magical equipment

Status: ready-for-agent

Gameplay requirements and test checklist: approved.
Implementation: on hold until the spec and tickets are ready; this document does not authorize implementation.

## Problem Statement

Traveling south currently provides narrative without a gameplay choice. Players need an optional opportunity to prepare for the dragon by choosing protection or increased damage.

## Solution

Every trip south opens a merchant menu. Each new character starts with 20 gold and no upgrades. Magical armor and a weapon enchantment each cost 15 gold. There are no additional gold sources: buying one upgrade leaves 5 gold, so the other is unaffordable for the remainder of that adventure.

The merchant displays current gold, both prices and effects, and ownership status. Choice 1 purchases armor, choice 2 enchants the existing weapon, and choice 3 leaves. Leaving costs nothing and returns to travel. End-of-input at the merchant behaves like Leave.

Armor subtracts 3 from damage on each successful, undefended dragon attack, with a minimum of 0. The enchantment adds 3 after the existing weapon damage roll on each successful, undefended player attack. Existing hit and defense checks remain unchanged.

## User Stories

1. As a player, I want every trip south to open the merchant so that I can choose whether to prepare.
2. As a player, I want to leave for free so that visiting does not commit me to a purchase.
3. As a player, I want 20 starting gold so that I can afford one upgrade.
4. As a player, I want visible prices and effects so that I can compare protection and damage.
5. As a player, I want armor to reduce incoming damage so that I can survive longer.
6. As a player, I want my existing weapon enchanted so that my occupation's weapon is preserved.
7. As a player, I want purchases equipped immediately so that I need no inventory management.
8. As a player, I want to see my remaining gold after buying so that I understand my purchasing power.
9. As a player, I want insufficient-funds attempts to preserve my gold and equipment so that failed purchases cost nothing.
10. As a player, I want owned upgrades visibly marked and duplicate selections free so that repeat visits cannot waste gold.
11. As a player, I want invalid input explained without changing state so that I can correct my choice.
12. As a player, I want the menu to remain open after every purchase attempt so that I choose when to leave.
13. As a player, I want gold and equipment retained across visits so that preparation lasts throughout the adventure.
14. As a player, I want each new character to begin without upgrades so that adventures are independent.
15. As a player, I want equipment effects visible in player and combat stats so that I can understand my capabilities.
16. As a player, I want combat messages to show actual damage so that the narration agrees with health changes.
17. As a player, I want merchant and equipment text in my selected language so that the feature fits the existing experience.
18. As a player, I want north to retain its existing ending behavior so that victory, defeat, and retreat conclude the adventure.

## Implementation Decisions

- Extend the existing travel flow with a merchant encounter, preserving the north encounter and existing hit/defense sequence.
- Maintain gold and upgrade ownership with the current adventure's player. A new character receives fresh starting state.
- Each upgrade costs 15 gold, equips immediately, lasts for the adventure, and can be owned only once. There is no stacking, durability, resale, refund, or additional gold source.
- Preserve the existing weapon type, ordinary damage roll, and occupation identity when enchanting it.
- Insufficient funds, invalid input, and duplicate purchases do not change gold or equipment. Owned items remain visible; duplicate selections explain ownership without charging.
- Keep the merchant menu open after successful and failed purchase attempts. Leave and merchant end-of-input return to travel.
- Display armor protection and weapon damage bonus in player and combat stats, and report damage after equipment effects.
- Use the existing JSON localization mechanism for English, Spanish, French, and Italian. Preserve existing canonical values and input commands, consistent with the accepted localization decision.
- Proposed testing seam, pending review: retain console input/output redirection for journey tests and introduce a controllable dice source shared by the game and its combat participants. Preserve normal random behavior by default. Prefer public behavior over private-method reflection or test-only combat rules.

## Testing Decisions

- Test externally visible purchase results, balances, ownership, health changes, navigation, and output; avoid assertions about private structure.
- Exercise merchant behavior, player and dragon attacks, equipment displays, and the complete travel flow.
- Existing tests provide console-redirection examples and combat checks; new exact combat assertions must use controlled rolls rather than probability or retry loops.
- Verify each purchase at sufficient and exactly 15 gold, leaving the expected balance and immediately activating the upgrade.
- Verify a normal 20-gold adventure leaves 5 gold after the first purchase and rejects the other upgrade. No repeat visit replenishes gold.
- Verify insufficient funds, duplicate selection within and across visits, invalid input, and leaving without spending preserve state.
- Verify the menu continues after every attempt, and Leave and merchant end-of-input return to travel.
- Verify fresh characters start with 20 gold and neither upgrade.
- Verify armor against damage below, equal to, and above 3, including zero damage without healing, and lethal damage retaining the existing nonnegative player-health behavior.
- Verify enchanted hits for fighter, magician, thief, and archer weapons, adding exactly 3 after the ordinary roll.
- Verify missed and defended attacks deal zero damage; verify ordinary attacks without upgrades retain their damage behavior.
- Verify exact health changes and actual-damage messages, plus equipment effects in player and combat stats.
- Verify south -> purchase -> leave -> north -> combat with controlled rolls, proving each upgrade persists and affects combat.
- Verify new localization keys exist with nonempty messages in all four languages and formatting placeholders match English, including occurrence counts.
- Run the existing test suite for regressions.

## Out of Scope

Additional gold sources, inventory management, replacement weapons, stacking, durability, resale, refunds, new combat rules, changes to northern encounter endings, and saved adventures across application runs.

## Further Notes

This is the authoritative merchant requirements specification, superseding the earlier south-merchant draft. The gameplay requirements and test checklist are approved. The proposed concrete dice-testing interface and ticket breakdown have not been explicitly approved. The ready-for-agent status records specification readiness and does not lift the implementation hold. No implementation or ticket publication is authorized by saving this specification. No architectural decision record is required for these reversible feature choices.

