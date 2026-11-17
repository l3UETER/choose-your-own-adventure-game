# 03: Buy magical armor and use it against the dragon

Status: ready-for-agent

**What to build:** Let players buy magical armor from the southern merchant, retain it on repeat visits, and receive its protection during the northern dragon encounter, with localized feedback and visible equipment effects.

**Blocked by:** 01: Make existing combat repeatably testable; 02: Visit and leave the southern merchant.

**Implementation hold:** Implementation requires explicit authorization; ready-for-agent describes ticket readiness only.

## Acceptance criteria

- [ ] Choice 1 buys armor for exactly 15 gold when affordable, equips it immediately, and displays the updated balance.
- [ ] A player starting with 20 gold has 5 gold after purchase; repeat visits never replenish that balance.
- [ ] Buying with exactly 15 gold succeeds and leaves 0. Buying with less than 15 changes neither gold nor equipment and explains insufficient funds.
- [ ] Owned armor remains visibly marked as owned. Selecting it again, within a visit or on a later visit, explains ownership and changes neither gold nor protection, even when funds are insufficient.
- [ ] Every purchase attempt keeps the merchant menu open until Leave or end-of-input.
- [ ] Armor reduces each successful, undefended dragon attack by exactly 3, with a minimum of 0 damage, without healing or changing existing hit and defense checks.
- [ ] Armor lasts for the adventure with no stacking, durability, resale, or refunds. A new character has no armor.
- [ ] Player and combat stats show armor protection; combat messages report damage after protection, matching the exact health change.
- [ ] Controlled-roll tests cover incoming damage below, equal to, and above 3, misses, defended attacks, ordinary damage without armor, and lethal reduced damage with nonnegative player health.
- [ ] A repeatable south -> buy armor -> leave -> revisit -> leave -> north -> combat test proves persistent ownership, gold, and reduced damage.
- [ ] Purchase, insufficient-funds, ownership, armor-stat, and any new combat text are localized in all four languages; coverage and placeholder-count tests pass.
- [ ] Existing tests pass.
