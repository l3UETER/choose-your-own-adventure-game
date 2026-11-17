# Category B: Inventory display

Status: ready-for-agent

## Problem Statement

Players cannot inspect their current equipment, merchant upgrades, and gold from the recurring travel menu. The character-stats menu is available before travel, so it does not provide a way to check later purchases while choosing a route.

## Solution

Offer an inventory display from the recurring travel menu through either `i` or `inventory`. Show the active weapon, magical armor, weapon enchantment, their current effects, and gold in the selected language. Immediately return to the travel menu without changing game state or requiring an acknowledgment.

## User Stories

1. As a player, I want inventory available at the travel menu, so that I can inspect my equipment before choosing a route.
2. As a player, I want the short command `i`, so that I can inspect inventory quickly.
3. As a player, I want the full command `inventory`, so that the command is easy to remember.
4. As a player, I want commands to ignore capitalization, so that uppercase and mixed-case input work.
5. As a player, I want surrounding whitespace ignored, so that accidental spaces do not reject my command.
6. As a player, I want to see my active weapon, so that I know what I attack with.
7. As a player, I want the weapon's maximum base damage, so that I understand its damage capability.
8. As a player, I want explicit labels for absent equipment or upgrades, so that an omitted line does not leave ownership ambiguous.
9. As a player, I want magical armor ownership and damage reduction shown, so that I understand my protection.
10. As a player, I want weapon enchantment status and its separate damage bonus shown, so that I can distinguish the upgrade from base weapon damage.
11. As a player, I want current gold displayed, so that I can assess merchant purchases.
12. As a player, I want inventory to reflect successful merchant purchases, so that I can verify my upgrades and remaining gold.
13. As a player, I want unsuccessful or duplicate purchases to leave the displayed inventory accurate, so that it reflects actual ownership.
14. As a player using English, Spanish, French, or Italian, I want localized inventory text and weapon names, so that the display follows my selected language.
15. As a player, I want to return immediately to travel, so that checking inventory does not add an acknowledgment step.
16. As a player, I want repeated checks to leave my adventure unchanged, so that inspection does not spend gold, trigger encounters, or alter combat outcomes.

## Implementation Decisions

- The confirmed feature belongs to the recurring travel menu, including when returning from the merchant or a surviving wolf encounter.
- Accept `i` and `inventory` after trimming surrounding whitespace and ignoring case. Advertise both tokens in the travel prompt. Keep these tokens unchanged across languages.
- The game controller must dispatch inventory against the current player and resume travel selection. Keep path-choice validation consistent with the actual accepted travel-menu commands.
- Display current player state rather than a purchase log: the single active weapon, magical armor ownership, weapon enchantment status, and gold.
- Show maximum base weapon damage separately from the enchantment damage bonus. Show the current armor reduction and bonus, including zero when the corresponding upgrade is absent.
- Use explicit localized absence text such as “None” for a missing weapon and “Not owned” for absent upgrades. Do not invent a base damage value for a missing weapon.
- Read existing player and weapon state; introduce no inventory collection, equipment slots, ownership history, or persistence schema.
- Use the existing message dictionary and weapon display translation conventions. Supply complete English, Spanish, French, and Italian text with matching formatting placeholders, consistent with the accepted Italian-localization decision.
- Exact wording and visual layout remain implementation choices subject to the required information and localization behavior.

## Acceptance Criteria

1. **Both commands:** At the recurring travel menu, entering `i` displays inventory once. In a separate equivalent case, `inventory` displays the same information. Neither is reported as invalid input or interpreted as a route.
2. **Case and whitespace:** Each of `I`, `INVENTORY`, `InVeNtOrY`, `  i  `, and a mixed-case full command surrounded by spaces or tabs behaves like its unpadded lowercase equivalent. Internal whitespace is not a new command alias.
3. **Discoverability:** The recurring travel prompt advertises both `i` and `inventory` in each supported language while retaining existing route and exit choices.
4. **Starting inventory:** For a newly created fighter before purchases, display long sword, maximum base damage 12, magical armor not owned with reduction 0, weapon enchantment not owned with bonus 0, and gold 20. Weapon identity and base damage also match the existing magician, thief, and archer assignments when those professions are selected.
5. **Absent weapon:** For a player whose weapon is absent, display an explicit localized absence label without throwing or inventing weapon ownership or base damage. Continue to report actual upgrade flags and gold.
6. **After magical armor:** Starting with 20 gold, purchase magical armor and leave the merchant. The next inventory check shows armor owned with reduction 3, gold 5, enchantment not owned with bonus 0, and the same weapon and base damage as before purchase.
7. **After weapon enchantment:** Starting with 20 gold, purchase the enchantment and leave the merchant. The next check shows enchantment owned with a separate bonus of 3, gold 5, armor not owned with reduction 0, and the original weapon and unchanged maximum base damage. Do not present the enchantment as a replacement or extra weapon.
8. **Both upgrades:** In a valid journey, earn the wolf's 10-gold reward and purchase both 15-gold upgrades. Inventory shows both owned, reduction 3, bonus 3, the unchanged weapon/base damage, and gold 0. Do not assume the initial 20 gold can buy both upgrades.
9. **Unsuccessful purchases:** Following an insufficient-funds attempt or an attempt to buy an already-owned upgrade, inventory reflects unchanged ownership and gold from immediately before that attempt.
10. **All languages:** Exercise both commands with English, Spanish, French, and Italian selected. Prompts, inventory labels, ownership/absence text, and weapon names use the selected language. No missing-key markers, empty required messages, or formatting errors appear; values and command behavior remain equivalent across languages.
11. **Immediate return:** After an inventory display, print the travel menu and accept the next input as a travel-menu command without consuming a “press Enter” acknowledgment. A script containing inventory followed directly by a route or exit must proceed correctly.
12. **Repeated access:** Consecutive inventory commands work, as do checks after leaving the merchant and after surviving a wolf encounter. Each check returns to travel selection.
13. **No state change:** Comparing otherwise identical deterministic journeys with and without inventory checks, player identity, attributes, health, weapon identity and properties, gold, upgrade flags, encounter health, reward eligibility, and subsequent encounter outcomes remain identical. Checks consume no random rolls, trigger no travel or attack, award no gold, and write no saves.
14. **Existing navigation:** North, south, west, exit, their existing abbreviations, invalid-input handling, and end-of-input behavior continue to work. Inventory is available only where specified, not added to startup, merchant, or combat menus.

## Testing Decisions

- Proposed primary seam: the existing public game-start entry point, using scripted console input, captured output, and deterministic dice. Prefer this single high-level seam over new public interfaces or private-method tests. This is a testing proposal, not an additional user-confirmed architecture decision.
- Test externally observable command acceptance, display contents, ordering of inventory and travel prompts, purchases, and subsequent journey outcomes. Avoid assertions tied to private helpers, exact decorative layout, or a particular decomposition.
- Exercise the game controller, player equipment/economy behavior, and localization together through representative journeys. Use parameterized cases for command spelling, case/whitespace, professions, and languages.
- Follow existing merchant and western-wolf journey tests for bounded input readers, deterministic rolls, console restoration, and protection against parallel console redirection. Compare journeys with and without inspection and require the same random-roll sequence to establish that inspection has no gameplay effect.
- Use existing message tests as prior art for translation coverage and placeholder parity. Supplement the high-level journeys only where a model state, such as a missing weapon, cannot be reached through normal character creation; prefer any display interface introduced by the implementation rather than exposing private internals solely for tests.
- During implementation, run the relevant inventory/localization/journey tests and the existing regression suite. No implementation tests are run as part of publishing this specification.

## Out of Scope

- New carried-item storage, spare weapons, clothing, accessories, consumables, or quantities.
- Equipping, unequipping, selling, dropping, or using items.
- Purchase history, new merchant offers, price changes, or combat balance changes.
- Save/load support or persistence changes.
- Inventory access from startup, character creation, merchant, or combat menus.
- An extra acknowledgment prompt after inspection.

## Further Notes

- The inventory interview was completed and its plan explicitly confirmed. This specification records that plan; implementation remains deferred until separately requested.
- Existing equipment and economy state lives on the current in-memory player. Magical armor applies automatically when owned; the model has no separate worn-versus-carried state. Weapon enchantment improves the existing weapon.
- Source context: `Player.cs` defines player equipment, gold, and derived effects; `Weapon.cs` defines weapon data; `Game.cs` owns the recurring travel loop and merchant purchase flow; `Wolf.cs` awards gold; `Messages.cs` and `language_data.json` provide localization. The domain vocabulary is in `GLOSSARY.md`; localization guidance is in `docs/0001-italian-language-support.md`.
- Testing prior art: `CYOATests/MerchantTests.cs`, `CYOATests/WesternWolfJourneyTests.cs`, `CYOATests/RepeatableCombatTests.cs`, and `CYOATests/MessagesTests.cs`.

## Validation

- Automated validation: full suite passed 270/270 tests (0 failed, 0 skipped). Implementation and journey coverage are committed in 08dd342 and 681aa8e.
- Manual validation: the user completed the Category B playthrough and confirmed inventory matched expected equipment and gold at every step. The user authorized committing these documents and pushing main to origin.
