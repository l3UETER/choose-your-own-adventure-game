# 01: Encapsulate merchant purchases in Player and verify gameplay

Status: ready-for-agent

**What to build:** Preserve the merchant experience for magical armor and weapon enchantment while moving purchase decisions and state changes into Player. Players continue paying 15 gold per upgrade, receive the same localized feedback, retain their existing weapon, and can use the wolf encounter reward to fund the second upgrade.

**Blocked by:** None (can start immediately).

**Spec:** [Part 4: Encapsulate merchant purchases in Player](../spec.md).

This is one complete slice: both upgrades, merchant integration, and verification through existing gameplay tests. The changes share the same purchase rules and can be completed and verified together; no separate prefactoring ticket is needed.

## Acceptance criteria

The spec expresses acceptance requirements through its User Stories, Implementation Decisions, and Testing Decisions. References below link to those sections and identify the relevant stories.

- [x] Player exposes one PurchaseUpgrade(Upgrade) operation returning PurchaseResult: Purchased, AlreadyOwned, or InsufficientGold. Support MagicalArmor and WeaponEnchantment. Game passes only the requested upgrade and delegates purchase decisions. ([Implementation Decisions](../spec.md#implementation-decisions); [User Stories](../spec.md#user-stories) 19.)
- [x] Check ownership before funds. Owned upgrades return AlreadyOwned without charging or changing state, including at zero gold and across repeated visits. ([User Stories](../spec.md#user-stories) 5–6; [Implementation Decisions](../spec.md#implementation-decisions).)
- [x] Both upgrades cost exactly 15 gold, with the shared price maintained in one place inside Player. An unowned upgrade at 15 gold returns Purchased, leaves zero gold, and immediately equips only the requested upgrade; at 14 or 0 gold it returns InsufficientGold with no state changes. ([User Stories](../spec.md#user-stories) 1–4, 7, 11; [Implementation Decisions](../spec.md#implementation-decisions).)
- [x] Weapon enchantment preserves the same weapon object, its properties, and the player's profession. Neither purchase changes health, the other upgrade, encounter state, or dice consumption. Armor protection and enchantment damage bonus remain 3 and do not stack. ([User Stories](../spec.md#user-stories) 8–10, 14; [Implementation Decisions](../spec.md#implementation-decisions).)
- [x] Game retains console interaction and selects existing localized messages from the requested upgrade and returned result. Player's purchase operation performs no console output or localization. Preserve existing language data and command tokens. ([User Stories](../spec.md#user-stories) 15; [Implementation Decisions](../spec.md#implementation-decisions).)
- [x] Preserve merchant input handling, invalid-choice behavior, leaving, closed input, menu repetition, and state persistence across visits. Invalid choices never invoke a purchase. ([User Stories](../spec.md#user-stories) 14, 16–18.)
- [x] Keep existing MerchantTests assertions for the 15/14/0 gold cases, ownership precedence at 30 and 0 gold, duplicate selections across visits, weapon identity, and both purchase orders. Verify using ArmorPurchase_RespectsAvailableGoldAndRemainsOpen, EnchantmentPurchase_RespectsFundsAndKeepsExistingWeapon, OwnedArmor_TakesPrecedenceOverFundsAndDoesNotCharge, OwnedEnchantment_NeverChargesOrStacksEvenAcrossVisits, and the existing revisit tests. ([Testing Decisions](../spec.md#testing-decisions); [User Stories](../spec.md#user-stories) 20.)
- [x] Keep and pass WesternWolfJourneyTests.BuyUpgradeWinWolfBuyOther_BothRemainActiveWithoutDuplicateCharges in both purchase orders. Verify 20 → 5 → 15 → 0 gold, one reward, one successful purchase of each upgrade, free duplicate attempts, and both effects in subsequent combat. Also retain WinBeforeShopping_ThirtyGoldBuysBothUpgradesAtExistingPrices for 30 → 15 → 0. ([User Stories](../spec.md#user-stories) 11–14; [Testing Decisions](../spec.md#testing-decisions).)
- [x] Keep inventory, localization, armor, enchantment, and repeated-combat coverage, including failed-purchase inventory equality and one-time reward eligibility. Run dotnet test and require the complete suite to pass without weakening or replacing existing assertions. Record the result; the pre-refactor baseline is 270 passing tests. ([Testing Decisions](../spec.md#testing-decisions); [Further Notes](../spec.md#further-notes).)
- [x] Keep the refactor within the approved scope: no new upgrades, prices, rewards, combat parsing, persistence, broad Player property changes, or unrelated cleanup. ([Out of Scope](../spec.md#out-of-scope).)

## Comments

- The user requested one ticket if both upgrades and verification fit together, and explicitly requested stopping before implementation. Creating this ticket does not start implementation.

- Implemented both upgrades through Player.PurchaseUpgrade; Game retains localized feedback. MerchantTests: 30 passed. Full dotnet test: 270 passed, 0 failed, 0 skipped. Existing tests and assertions unchanged. Manual merchant playthrough pending; push held at the user's request.
