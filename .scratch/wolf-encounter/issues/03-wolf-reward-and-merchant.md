# 03: Award wolf victory gold and enable both merchant upgrades

Status: ready-for-agent
Workflow: resolved

**What to build:** Defeating the wolf awards 10 gold once, displays the reward, and lets the player use that gold at the merchant. With 20 starting gold plus the reward, the player can purchase both magical armor and weapon enchantment at their existing 15-gold prices.

**Blocked by:** [02: Fight or retreat from a persistent western wolf](02-persistent-western-wolf.md).

**Execution hold:** Lifted by the user for Ticket 03. The user completed the manual playthrough and authorized pushing after tests and review pass.

## Source and acceptance mapping

[Approved specification: Accepted validation checklist](../spec.md#accepted-validation-checklist), item 5, the reward-output portion of item 7, and items 8–9. Also verify persistence from items 2 and 4 across the rewarded journey. Item numbers refer to the bullets in that checklist, in order.

- [x] Award exactly 10 gold when the wolf is defeated, once per adventure, and preserve it when returning to travel.
- [x] Initial retreat, in-combat retreat, player defeat, invalid input, closed input, and later clear-route visits award no gold. A fresh adventure resets reward eligibility along with wolf state.
- [x] Display a wolf-specific victory reward message through the existing localization mechanism, in English, Spanish, French, and Italian; verify nonempty text and matching English placeholder counts and indices.
- [x] Verify a deterministic journey: start with 20 gold, buy one 15-gold upgrade, defeat the wolf, then buy the other upgrade. Balances are 5 after the first purchase, 15 after the reward, and 0 after the second purchase.
- [x] Cover either purchase order and a journey that defeats the wolf before any merchant purchase, confirming 30 total available gold and both upgrades can be obtained without changing prices.
- [x] Both upgrades remain active for subsequent combat; preserve the original weapon identity and remaining player health across travel and merchant visits.
- [x] Repeat visits cannot duplicate the reward, replenish gold, or charge again for already-owned equipment. Existing insufficient-funds, ownership, and leave behavior remains intact.
- [x] Run the existing test suite and deterministic integration checks for reward persistence, merchant purchases, and northern combat endings.

## Intentional requirement change

The approved wolf specification expands the earlier merchant design's prohibition on additional gold sources. The one-time wolf reward is the authorized new source; starting gold, prices, equipment effects, and duplicate-purchase protection remain unchanged. Ticket 01 is independent and is not a blocker for this reward slice.

## Comments

The first deterministic reward journey failed because the post-victory merchant balance remained 20. Adding 10 to the same Player.Gold balance in the first victory branch made that test pass. A reward-output test then failed before the localized message was added in all four languages. Cleared-route visits return before the victory branch, preventing repeat rewards without another state flag.

Verified both purchase orders with balances 5, 15, and 0; victory before shopping with balances 30, 15, and 0; repeated west and owned-item visits; no reward on retreat, invalid or closed input, or defeat; fresh-adventure reward eligibility; original weapon identity, wounds, and both equipment effects in subsequent combat. Updated Ticket 02's temporary no-reward expectations to the approved reward behavior.

Validation: 42 focused wolf cases passed. Full dotnet test passed 203 tests with zero failures or skips, including northern endings and existing merchant behavior. Push held for the user's manual playthrough.

The user subsequently confirmed that the manual Category A playthrough passed: the wolf encounter, one-time gold reward, and both merchant upgrades worked. The push hold is lifted, subject to passing tests and review and verifying main and the fork remote.
