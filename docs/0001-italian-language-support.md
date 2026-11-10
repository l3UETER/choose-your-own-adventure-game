# ADR 0001: Add Italian using the existing JSON localization structure

Status: Accepted

Date: 2026-11-09

## Context

The console adventure game supports English, Spanish, and French through
`language_data.json`. Each language has a `dictionary` of messages, input maps
(`raceMap` and `occupationMap`), and display maps (`displayRaceMap`,
`displayOccupationMap`, and `displayWeaponMap`). The `Messages` class loads the
selected language, looks up messages by key, and translates between localized
labels and the canonical English values used by game logic.

Italian support needs to cover the same prompts, narrative, combat messages,
and character and weapon labels while preserving the existing language options.

## Decision

Reuse the existing JSON structure by adding a top-level `Italian` entry to
`language_data.json`. Use English as the reference for message keys, formatting
placeholders, map coverage, and canonical values. Translate message text and
display labels into Italian. Input maps accept Italian race and profession
names and return the same canonical values as the other languages; for example,
`umano` maps to `Human` and `guerriero` maps to `Fighter`.

In `Game.StartGame()`, display `4. Italiano` alongside the existing options.
The language-selection switch maps input `"4"` to `"Italian"`, matching the
JSON entry name. The game passes that value to
`messages.SetCurrentLanguage(selectedLanguage)` and then calls
`messages.ReadDictionary()`. That method reads `language_data.json` from
`AppContext.BaseDirectory` and selects the section named by `CurrentLanguage`.
Existing calls to `GetMessage` and the display translation methods then use
Italian data without changes to their callers or to the loader.

Keep English as the default and retain Spanish and French as options 2 and 3.
Keep existing command tokens such as `roll`, `a`, `r`, `n`, `s`, and `e`, and
show those tokens in the Italian prompts because input handling expects them.

## Consequences

The change uses the existing loader and lookup methods, so it requires no new
localization framework or language-specific game logic. Canonical values remain
stable across languages, allowing the same character and combat rules to work.

The trade-off is manual synchronization: new message keys must be added to every
language, and each translated format string must preserve the reference
placeholders, including their indices and occurrences, such as `{0}` and `{1}`.
Their positions in a sentence may change to suit the translation. Missing keys
appear as `[key]`; incorrect placeholders can produce incorrect output or a
`FormatException` when callers use `string.Format`.

Translation maps also need equivalent coverage whenever a race, profession, or
weapon is added. Input-map keys are localized, while their canonical values and
the display-map keys must continue to match game logic.

For this change, a separate validation check confirmed all 69 Italian message
keys, all five translation maps, nonempty translations, and matching formatting
placeholders against English. Existing language entries were unchanged, and
`dotnet test` passed all 94 tests. These one-time checks do not automatically
enforce synchronization for future edits.
