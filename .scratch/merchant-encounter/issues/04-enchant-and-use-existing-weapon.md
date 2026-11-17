# 04: Enchant the existing weapon and enforce the one-upgrade budget

Status: ready-for-agent

**What to build:** Let players enchant their occupation's existing weapon, retain the bonus across visits, and use it against the dragon. Complete the merchant's mutually exclusive purchasing choice under the fixed adventure budget.

**Blocked by:** 01: Make existing combat repeatably testable; 02: Visit and leave the southern merchant; 03: Buy magical armor and use it against the dragon (required to verify both purchase orders and the complete budget constraint).

**Implementation hold:** Implementation requires explicit authorization; ready-for-agent describes ticket readiness only.

## Acceptance criteria

- [ ] Choice 2 enchants the existing weapon for exactly 15 gold when affordable, equips the bonus immediately, and displays the updated balance.
- [ ] Preserve the weapon's type, occupation identity, ordinary damage roll, and existing hit and defense checks.
- [ ] Each successful, undefended player attack adds exactly 3 after the normal damage roll. Misses and defended attacks deal 0 damage.
- [ ] Controlled-roll tests assert exact damage and dragon-health changes for fighter, magician, thief, and archer weapons, and unchanged damage without enchantment.
- [ ] Exact-price purchases leave 0 gold; insufficient funds change neither gold nor equipment and display a clear explanation.
- [ ] Owned enchantments remain visible as owned. Repeated selections within and across visits charge nothing and never stack the bonus, including when funds are insufficient.
- [ ] Every purchase attempt keeps the menu open until Leave or end-of-input.
- [ ] Player and combat stats show the fixed weapon damage bonus; combat messages show actual damage including that bonus.
- [ ] A repeatable south -> enchant -> leave -> revisit -> leave -> north -> combat test proves the enchantment remains active.
- [ ] Test both purchase orders from 20 starting gold: the first purchase leaves 5; the other purchase is rejected with no state change, including after revisiting. No additional gold source, refund, or resale permits buying both.
- [ ] A new character starts with 20 gold and neither upgrade; an enchantment lasts only for its adventure and has no durability.
- [ ] All new purchase, ownership, equipment, and combat text uses the existing JSON localization structure in all four languages, with nonempty-message and matching-placeholder tests.
- [ ] Run the complete existing and new test suite; preserve northern victory, defeat, and retreat endings.
