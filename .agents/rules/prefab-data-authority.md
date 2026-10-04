---
trigger: model_decision
description: "Apply when creating or modifying prefabs, ScriptableObjects, runtime state, registries, content definitions, serialized references, or authoring workflows."
---

# Prefab, Definition, and Runtime-State Authority

## Definition Layer
ScriptableObjects describe authored facts and configuration.
Examples:
- `CoffeeRecipeDefinition`
- `IngredientDefinition`
- `CustomerDefinition`
- `FurnitureDefinition`
- `CafeEventDefinition`
- `UpgradeDefinition`
- `MusicTrackDefinition`
- `SocialPostTemplate`

Definitions should contain stable IDs and authoring references.

## Prefab Layer
Prefabs contain authored scene entities and presentation:
- visuals,
- colliders,
- sockets/anchors,
- animation components,
- interaction components,
- presentation adapters.

A definition may reference a prefab. The prefab should not become a database.

## Runtime Layer
Mutable state belongs in runtime objects/save data, not definitions.
Examples:
- current customer familiarity,
- current money,
- current stock,
- active order status,
- current shift metrics,
- unlocked content.

## Reference Rules
- Prefer serialized direct references for authored dependencies.
- Do not use arbitrary `Resources.Load("string/path")` as primary architecture.
- Do not infer missing references from names.
- Do not auto-link similarly named assets without explicit user authorization.
- Missing references should be detectable by validation tools.

## Prefab Editing
Before modifying a prefab:
1. inspect the exact prefab,
2. identify whether it is vendor-owned or project-owned,
3. if vendor-owned and modification is required, create a project-owned variant/derivative,
4. make the smallest targeted edit,
5. preserve overrides intentionally.

## Stable IDs
Save-facing definitions need stable unique IDs.
- Never regenerate IDs casually.
- Never derive persistent identity only from display names.
- Provide editor validation for duplicate/missing IDs.
