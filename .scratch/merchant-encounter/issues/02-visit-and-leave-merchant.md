# 02: Visit and leave the southern merchant

Status: ready-for-agent

**What to build:** Let players visit the optional merchant on every trip south, inspect both offers and their gold, and leave for free to continue traveling. Establish the per-adventure economy and localized interaction used by the purchase tickets.

**Blocked by:** None (no ticket dependencies).

**Implementation hold:** Stop at planning until implementation is explicitly authorized. Purchase actions and equipment effects are delivered by tickets 03 and 04; this slice establishes visit, display, and leave behavior.

## Acceptance criteria

- [ ] Every new character starts with 20 gold and no magical armor or weapon enchantment.
- [ ] Every trip south opens the merchant menu; leaving returns to travel without spending gold or changing equipment.
- [ ] The menu shows current gold and both offers, each priced at 15 gold, with effects and ownership status: 1 armor, 2 enchant the existing weapon, 3 leave.
- [ ] The displayed effects are 3 incoming damage reduction with a minimum of 0, and a fixed 3 damage bonus on successful, undefended hits.
- [ ] Invalid input displays a clear message, changes no state, and keeps the menu open.
- [ ] End-of-input at the merchant behaves like Leave and does not loop indefinitely.
- [ ] Revisiting does not replenish gold or reset equipment. There are no additional gold sources.
- [ ] Merchant navigation, offer text, prices, effects, gold labels, and ownership labels use the existing JSON localization structure in English, Spanish, French, and Italian.
- [ ] Repeatable scripted tests cover south -> leave -> south -> leave -> north, invalid input, leaving without spending, merchant end-of-input, and independent starting state for new characters.
- [ ] Localization tests verify all new messages are nonempty in all four languages and formatting placeholders match English, including occurrence counts.
- [ ] Existing tests pass, and north retains its existing victory, defeat, and retreat endings.
