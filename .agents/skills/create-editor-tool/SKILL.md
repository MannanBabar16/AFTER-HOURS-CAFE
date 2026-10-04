---
name: create-editor-tool
description: "Creates focused Unity editor tooling for After Hours Café to reduce repetitive authoring, improve validation, accelerate playtesting, and keep project data safe."
---

# Create Editor Tool

## Use When
Use this skill when a gameplay/content system has repeated authoring work, recurring configuration errors, bulk editing needs, or painful playtest setup.

## First Decide If a Tool Is Worth It
A custom tool should save future time or reduce error.
Do not build one for a one-off edit.

## Design
Define:
- target users/tasks,
- data/assets it may modify,
- preview/validation needs,
- Undo requirements,
- search/filter requirements,
- whether a custom inspector or EditorWindow is more appropriate.

## Technology
- Prefer UI Toolkit for substantial windows.
- Use standard inspector/IMGUI APIs when simpler.
- Keep editor code editor-only.
- Use the `Mannan/After Hours Café` menu root.

## Safety
- Inspect ownership before modification.
- Never mutate vendor originals.
- Preserve explicit prefab/model/material references.
- Use Undo and serialized APIs where appropriate.
- Make bulk actions previewable/clear.
- Mark dirty/save intentionally.

## Quality
Good tools should provide:
- search/filter,
- clear labels,
- compact layout,
- actionable validation,
- selection/ping of problem assets,
- no hidden destructive behavior.

## Validate
Test the tool against representative valid and invalid content before declaring it complete.
