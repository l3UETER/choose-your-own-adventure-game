# 02: Inspect inventory across merchant and wolf journeys

Status: ready-for-agent

**What to build:** Complete and verify the inventory experience across merchant purchases and surviving wolf encounters. Players see accurate current equipment and gold after successful, duplicate, or unaffordable purchases, and repeated inspection leaves the adventure unchanged. Fix any integration gaps revealed by these journeys.

**Blocked by:** 01 — [Inspect current inventory from travel](01-inspect-current-inventory-from-travel.md).

**Specification:** [Category B inventory display](../spec.md).

**Acceptance criteria covered:** [Spec acceptance criteria](../spec.md#acceptance-criteria), AC 6-9 and 12-13. Retain the command, localization, navigation, and immediate-return guarantees established by ticket 01.

- [x] **AC 6:** From 20 gold, buy magical armor and leave the merchant. Inventory shows armor owned/reduction 3, gold 5, enchantment not owned/bonus 0, and the unchanged weapon and maximum base damage.
- [x] **AC 7:** From 20 gold, buy weapon enchantment and leave the merchant. Inventory shows enchantment owned/separate bonus 3, gold 5, armor not owned/reduction 0, and the original weapon and unchanged maximum base damage. No replacement or extra weapon is implied.
- [x] **AC 8:** Through a valid journey, earn the wolf's 10-gold reward and purchase both upgrades. Inventory shows both owned, reduction 3, separate bonus 3, the unchanged weapon/base damage, and gold 0. Do not bypass the starting economy to demonstrate the journey.
- [x] **AC 9:** After insufficient-funds and already-owned purchase attempts, inventory reports the same ownership and balance as immediately before those attempts.
- [x] **AC 12:** Consecutive checks work and return to travel, including after leaving the merchant and after surviving a wolf encounter. Exercise both command spellings across these journeys.
- [x] **AC 13:** Compare equivalent deterministic journeys with and without inventory checks. Player identity, attributes, health, weapon identity/properties, gold, upgrade flags, encounter health, reward eligibility, and subsequent outcomes remain unchanged by inspection.
- [x] **AC 13:** Verify inventory consumes no random rolls, triggers no travel or attack, awards no gold, and writes no saves. Repeated visits to an already-defeated wolf route must not gain another reward because inventory was checked.
- [x] Retain all ticket 01 acceptance behavior, including all four languages and direct return to travel without an acknowledgment step. Do not change merchant prices, combat rules, or reward rules to satisfy inventory scenarios.

## Testing approach

Extend the high-level scripted-journey coverage established in ticket 01. Use existing deterministic dice and bounded-console patterns, comparing transcripts and later observable outcomes against journeys without inventory. Require the same random-roll sequence with and without inspection; verify upgraded combat effects and reward eligibility through subsequent behavior rather than private-method assertions.

Prefer this existing high-level seam and supplement it only where the spec's state-preservation checks cannot be established through observable journeys. Restore console state and prevent concurrent console redirection. Run relevant inventory, merchant, wolf, and localization tests plus the regression suite. Keep this slice focused on integration behavior and fixes, with no new inventory model or persistence.

## Comments

- 2026-11-16: First test checked inventory after a real magical-armor purchase and passed with existing game code. Added scenarios incrementally for enchantment, insufficient funds, duplicate purchases, wolf-funded upgrades, repeated inspection, and reward eligibility. No production changes were needed.
- Deterministic journeys in all four languages compare complete non-inspection transcripts with identical exhausted dice queues. They retain wolf wounds after retreat, player wounds into dragon combat, upgraded damage/reduction, gold and reward eligibility. Existing Ticket 01 display tests retain public player state and weapon object identity. An isolated working-directory check confirms no save files created or modified there; source review confirms inventory has no persistence writes.
- Validation: full dotnet test suite passed 270/270 (0 failed, 0 skipped), including all Ticket 01 regressions. Command: dotnet test --no-restore -p:OutputPath=bin/InventoryRed/ -p:UseAppHost=false. Standards review's redundant-test suggestion was resolved; Spec review found no gaps. git diff --check passed.
- Push remains on hold until the user completes the manual playthrough and authorizes pushing.

- Manual validation completed: the user confirmed expected equipment and gold at every Category B playthrough step and authorized pushing main to origin. This supersedes the earlier push hold.
