# 01: Make existing combat repeatably testable

Status: ready-for-agent

**What to build:** Preserve the current combat experience while allowing tests to drive exact dice outcomes through ordinary gameplay and attack behavior. This preparatory refactor supports precise equipment and journey tests without changing game rules.

**Blocked by:** None (no ticket dependencies).

**Implementation hold:** Ticket creation does not authorize implementation. The spec's proposed concrete dice interface remains subject to review; controlled outcomes are required, but no specific interface is mandated here.

## Acceptance criteria

- [ ] Tests can control player and dragon attack, defense, and damage rolls, including character and dragon initialization when exercising the full journey.
- [ ] Reuse existing console input/output redirection for scripted journeys and restore console state after tests.
- [ ] Use the smallest practical randomness seam, preserving normal random behavior by default and existing callers where practical.
- [ ] Assert exact health changes for ordinary successful, undefended player and dragon attacks without upgrades.
- [ ] Assert misses and defended attacks cause no health loss, and lethal incoming damage keeps player health nonnegative.
- [ ] Tests exercise observable behavior rather than private-method reflection, probabilistic retries, or separate test-only combat rules.
- [ ] Existing tests pass without changing existing hit checks, defense checks, damage ranges, or encounter endings.
