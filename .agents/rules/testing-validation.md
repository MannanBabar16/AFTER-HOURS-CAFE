---
trigger: model_decision
description: "Apply when validating gameplay systems, writing tests, changing save data, adding validators, debugging regressions, or declaring implementation complete."
---

# Testing and Validation Rules

## Validation Levels
Use the lightest level that proves the change:
- compile/console check,
- targeted inspector/data validation,
- EditMode test,
- PlayMode test,
- manual gameplay smoke test,
- profiling for performance claims.

## Logic Tests
Prefer EditMode tests for:
- recipe calculations,
- economy math,
- progression conditions,
- customer preference scoring,
- state transitions,
- save serialization/migration,
- ID/definition validators.

## PlayMode Tests
Use when Unity lifecycle/integration matters:
- interaction flow,
- prefab wiring,
- customer lifecycle integration,
- shift start/end,
- UI binding smoke tests.

## Save Data
Save schema must be versioned.
When schema changes:
- preserve compatibility when reasonable,
- add migration/default behavior,
- test old/new representative data where possible.

## Completion
A task is not "verified" simply because code compiles.
State what was actually tested.
If Play Mode or the relevant scene could not be run, say so explicitly.
