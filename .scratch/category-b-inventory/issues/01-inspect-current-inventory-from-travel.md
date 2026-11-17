# 01: Inspect current inventory from travel

Status: ready-for-agent

**What to build:** Let players inspect current equipment, upgrades, their effects, and gold from the recurring travel menu using either inventory command. Provide the display in every supported language and immediately resume travel selection without changing game state.

**Blocked by:** None (can start immediately).

**Specification:** [Category B inventory display](../spec.md).

**Acceptance criteria covered:** [Spec acceptance criteria](../spec.md#acceptance-criteria), AC 1-5, 10-11, and 14. AC numbers below refer to that numbered list.

- [x] **AC 1:** Both `i` and `inventory` display inventory once from travel, with equivalent information and no invalid-input or route action.
- [x] **AC 2:** Ignore case and surrounding spaces or tabs, including `I`, `INVENTORY`, `InVeNtOrY`, and padded inputs. Do not introduce an internal-whitespace alias.
- [x] **AC 3:** Advertise both tokens in the recurring travel prompt in all four languages while retaining route and exit choices.
- [x] **AC 4:** A starting fighter displays long sword, maximum base damage 12, armor not owned/reduction 0, enchantment not owned/bonus 0, and gold 20. Verify the existing weapon and base damage for magician, thief, and archer as well.
- [x] **AC 5:** A missing weapon produces an explicit localized absence label without exceptions or invented weapon/base damage, while reporting actual upgrade flags and gold.
- [x] **AC 10:** Both commands work in English, Spanish, French, and Italian. Localize labels, ownership/absence text, prompts, and weapon names; retain `i` and `inventory` as tokens. Verify nonempty messages, no missing-key markers, valid formatting, and placeholder parity.
- [x] **AC 11:** Immediately redisplay travel selection after inventory and process a directly following route or exit without an acknowledgment input.
- [x] **AC 14:** Preserve existing routes, abbreviations, invalid-input handling, and end-of-input behavior. Do not add inventory to other menus. Keep travel-command validation consistent with actual dispatch.
- [x] Display current state, including separately labeled maximum base weapon damage, armor reduction, and enchantment bonus. Avoid hardcoded starting values; support current ownership and gold so the follow-up journey ticket can verify purchases without redesigning the display.
- [x] Keep inspection free of gameplay mutations, random rolls, and save writes. The follow-up ticket verifies this across journeys.

## Testing approach

Prefer scripted console journeys through the existing public game-start entry point, with bounded input, captured output, deterministic dice, restored console state, and nonparallel console tests. Parameterize commands, professions, and languages. For the unreachable-through-creation missing-weapon case, use a display interface introduced by the implementation if needed; do not expose private internals solely for tests.

Follow existing merchant, journey, and localization test patterns. Assert observable information and navigation rather than decorative formatting or private helper structure. Run relevant tests and the regression suite. Respect the accepted localization decision and the spec's scope; add no inventory storage, equipment management, or persistence.

## Comments

- 2026-11-16: Completed Ticket 01 using incremental red/green tests, then review-driven refactoring. Both inventory aliases share the travel parser and display live player state in all four languages. Missing weapons show localized absence without base damage. No gameplay mutations, random rolls, or persistence were added.
- Validation: 49 inventory test cases passed; full dotnet test suite passed 252/252 (0 failed, 0 skipped). Used --no-restore -p:OutputPath=bin/InventoryRed/ -p:UseAppHost=false to avoid the running game's locked default output files. Compilation and git diff --check passed. Standards and Spec reviews have no remaining findings.
- Stopped before Ticket 02. Purchase journeys and broader encounter-state comparisons remain in that ticket.

- Manual validation completed: the user confirmed expected equipment and gold at every Category B playthrough step and authorized pushing main to origin. This supersedes the earlier push hold.
