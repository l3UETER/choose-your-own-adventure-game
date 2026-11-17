# Part 4: Encapsulate merchant purchases in Player

Status: ready-for-agent

Design approved. Implementation complete; all 270 existing tests pass. Manual merchant playthrough pending; push held.

## Problem Statement

The merchant menu duplicates ownership checks, affordability checks, gold deductions, and equipment changes for magical armor and weapon enchantment. Maintaining these rules in the menu requires knowledge of Player state and risks inconsistent purchases as gameplay evolves. Players need their existing purchases, balances, equipment, and localized feedback preserved during the refactor.

## Solution

Give the Player module one purchase operation that owns the purchase rules and state changes for both upgrades. The Game module continues reading merchant input and displaying localized messages. All observable gameplay remains unchanged, including the ability to fund the second upgrade with the wolf encounter's one-time reward.

## User Stories

1. As a player, I want magical armor to cost 15 gold, so that its price remains familiar.
2. As a player, I want a weapon enchantment to cost 15 gold, so that its price remains familiar.
3. As a player, I want a purchase with exactly 15 gold to succeed and leave zero gold, so that the affordability rule is predictable.
4. As a player, I want a purchase with insufficient gold to leave my balance and equipment unchanged, so that failed attempts cost nothing.
5. As a player, I want an already-owned upgrade to be reported before insufficient funds, so that feedback accurately explains why another purchase is unnecessary.
6. As a player, I want duplicate purchases to cost nothing across repeated selections and visits, so that my gold is protected.
7. As a player, I want upgrades to apply immediately, so that I can use them in the next encounter.
8. As a player, I want a weapon enchantment to preserve my existing weapon and profession, so that my character's equipment remains intact.
9. As a player, I want repeated enchantment attempts to leave the damage bonus at 3, so that upgrades do not stack.
10. As a player, I want magical armor to retain its damage reduction of 3, so that combat remains unchanged.
11. As a player, I want both upgrade purchase orders to work, so that my first choice does not restrict later preparation.
12. As a player, I want the wolf encounter's 10-gold reward to fund my second upgrade after buying the first, so that successful exploration still improves preparation.
13. As a player, I want the wolf reward granted only once, so that revisiting the cleared route cannot fund extra purchases.
14. As a player, I want both upgrades to remain active through travel, inventory inspection, and subsequent combat, so that preparation persists.
15. As a player, I want merchant prompts, ownership labels, and feedback in my selected language, so that the refactor does not change localization.
16. As a player, I want invalid input and leaving the merchant to preserve state, so that menu navigation stays safe.
17. As a player, I want the merchant menu to remain open after purchase attempts, so that I decide when to leave.
18. As a player, I want closed merchant input to return to travel as before, so that scripted and interactive journeys retain their behavior.
19. As a maintainer, I want purchase decisions concentrated in Player, so that menu code need not coordinate gold deductions and ownership changes.
20. As a maintainer, I want existing gameplay tests retained, so that the refactor is verified against established behavior.

## Implementation Decisions

- Add one Player operation, PurchaseUpgrade(Upgrade), returning PurchaseResult. Upgrade identifies MagicalArmor or WeaponEnchantment; PurchaseResult is Purchased, AlreadyOwned, or InsufficientGold.
- Game translates a valid merchant purchase choice into the appropriate Upgrade and passes only that value. It does not pass a price, balance, Messages instance, or console input to the purchase operation.
- Player checks whether the requested upgrade is owned first. If owned, return AlreadyOwned without changing state, even when gold is below the price.
- If unowned and gold is below 15, return InsufficientGold without changing any state.
- Otherwise deduct exactly 15 gold, set only the requested upgrade's ownership flag, and return Purchased. Keep the shared price in one place inside Player's purchase implementation.
- Enchantment must retain the same weapon object, weapon properties, and profession. Neither purchase changes health, the other upgrade, encounter state, or random-roll consumption.
- Game maps the requested upgrade and purchase result to the existing localized feedback keys. Player's purchase operation performs no message lookup or console output.
- Keep menu rendering, input validation, leaving, end-of-input handling, and repeated visits in Game. Invalid menu input never invokes a purchase.
- Preserve the existing equipment properties and their combat effects. This small refactor does not redesign all Player state access.
- The purchase interface creates depth by hiding purchase ordering and mutations; locality concentrates these rules in Player, and leverage lets Game delegate the whole decision. The seam remains in-process; no adapter abstraction is needed.
- Preserve the accepted localization decision: retain the JSON structure, canonical values, existing messages, languages, and command tokens.

## Testing Decisions

- The user approved retaining existing gameplay tests as the verification surface. Prefer the existing merchant-visit and full-journey seams; no new test-only seam or console abstraction is required.
- Keep all existing tests and assertions. Test externally observable state, messages, weapon identity, and journey outcomes rather than private helper structure or call counts.
- Retain MerchantTests.ArmorPurchase_RespectsAvailableGoldAndRemainsOpen: 15 gold becomes 0 and equips armor; 14 and 0 remain unchanged with no armor and insufficient-funds feedback.
- Retain MerchantTests.EnchantmentPurchase_RespectsFundsAndKeepsExistingWeapon: the same balance cases verify the enchantment price, unchanged armor ownership, weapon object identity, and profession.
- Retain MerchantTests.OwnedArmor_TakesPrecedenceOverFundsAndDoesNotCharge and OwnedEnchantment_NeverChargesOrStacksEvenAcrossVisits: test owned upgrades at both 30 and 0 gold, unchanged balances, ownership feedback rather than insufficient-funds feedback, and a fixed enchantment bonus of 3.
- Retain MerchantTests.BuyArmor_RevisitAndSelectAgain_ChargesOnlyOnceAndKeepsMenuOpen and EitherPurchaseOrder_AllowsOnlyFirstUpgradeAcrossVisits for repeated visits and unaffordable second purchases before a reward.
- Retain WesternWolfJourneyTests.BuyUpgradeWinWolfBuyOther_BothRemainActiveWithoutDuplicateCharges in both purchase orders. Verify the balance progression 20 to 5 after the first purchase, 15 after the wolf reward, and 0 after the second purchase. The reward and each purchase succeed once; duplicate attempts preserve zero gold; both upgrades still affect subsequent combat.
- Retain WesternWolfJourneyTests.WinBeforeShopping_ThirtyGoldBuysBothUpgradesAtExistingPrices: both purchase orders produce 30 to 15 to 0 gold.
- Retain InventoryJourneyTests, especially FailedPurchases_LeaveInventoryIdenticalToBeforeTheAttempt and WolfReward_FundsBothUpgradesAndInventoryCannotRestoreRewardEligibility, to verify state persistence and failed-purchase behavior through travel.
- Retain the armor, enchantment, merchant localization, and repeated-combat coverage. Purchase refactoring must not alter damage, reward eligibility, menu timing, or dice sequences.
- Run dotnet test after implementation and require the complete suite to pass. Do not weaken or replace journey assertions to accommodate the refactor.

## Out of Scope

- New upgrades, price changes, discounts, repeatable upgrades, refunds, or a merchant catalog framework.
- Combat-choice parsing, encounter restructuring, changes to wolf rewards, or altered starting gold.
- Localization changes, new inventory behavior, persistence, dependency injection frameworks, or adapter abstractions.
- Broad changes to Player property visibility or unrelated cleanup.
- Implementing the refactor as part of this spec-writing request.

## Further Notes

- Before this spec was written, dotnet test passed all 270 tests with 0 failures and 0 skips after the running game was closed. This is the pre-refactor baseline, not evidence of an implementation.
- The user explicitly approved the single purchase operation, three result values, ownership-first ordering, 15-gold price, and localized feedback remaining in Game.
- Current gameplay includes the one-time wolf reward; older merchant-only requirements stating that no additional gold source exists do not describe the current adventure.
