# Domain docs

This repo uses a single-context layout:

- `GLOSSARY.md` at the repo root for domain vocabulary.
- `docs/adr/` for new architectural decision records.

## Before exploring

Read `GLOSSARY.md`, when present, and ADRs relevant to the work.

Also consult the existing ADR at
`docs/0001-italian-language-support.md` for localization decisions.

If the glossary or ADR directory does not exist, proceed silently.
The domain-modeling skill creates domain documents when terms or
decisions are resolved.

## Use the glossary's vocabulary

Use glossary terms in issue titles, proposals, hypotheses, and test names.
Respect any distinctions between preferred terms and avoided synonyms.

When a needed concept is absent, reconsider whether it belongs to the
domain. Note real vocabulary gaps for domain-modeling.

## Surface ADR conflicts

If a proposal contradicts an accepted ADR, identify the ADR and explain
why the decision should be reconsidered.
