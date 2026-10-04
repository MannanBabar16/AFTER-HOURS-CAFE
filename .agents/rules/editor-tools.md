---
trigger: model_decision
description: "Apply when creating custom inspectors, EditorWindows, authoring utilities, content browsers, validators, playtest/debug panels, bulk tools, or other Unity editor automation for the Mannan project."
---

# Editor Tooling Rules

Editor tooling is a first-class development feature for this project.

## Purpose
Build tools when they:
- remove repeated manual work,
- make content authoring safer,
- make data relationships visible,
- validate common mistakes,
- accelerate playtesting,
- reduce AI/user error.

Do not build elaborate tooling for a one-time task.

## Main Menu
Prefer a coherent root:
`Mannan/After Hours Café/...`

Long-term, consolidate major tools into a central window with sections such as:
- Dashboard
- Coffee
- Customers
- Furniture
- Social
- Events
- Progression
- Save
- Debug
- Validation

## UI Technology
Prefer UI Toolkit for substantial editor windows.
Use standard IMGUI/custom inspector APIs when they are simpler for a small inspector.

## Authoring Tool Expectations
Useful system-specific tools may include:
- Coffee Recipe Editor
- Customer Authoring Browser
- Furniture/Style Database
- Shift/Event Editor
- Social Template Browser
- Progression/Unlock Graph
- Save Inspector
- Playtest Console
- Project Validator

## Safety
Editor tools that modify assets must:
- support Undo where appropriate,
- use `SerializedObject`/`SerializedProperty` where appropriate,
- mark assets/scenes dirty intentionally,
- show what will be changed for broad operations,
- never modify vendor originals,
- preserve explicit user references.

## Validation
A central validator should eventually detect:
- missing references,
- duplicate IDs,
- missing IDs,
- invalid recipe references,
- broken unlock dependencies,
- missing prefabs/icons,
- invalid customer preferences,
- save-definition mismatches.

Validation should link/select offending assets when practical.

## Playtest Tool
A development-only tool may provide controlled actions:
- set money/reputation,
- set time,
- set weather,
- spawn selected customer,
- trigger event,
- unlock selected/all recipes,
- trigger social post,
- inspect active orders,
- save/load/reset development profile.

Do not ship development cheats in production builds unless explicitly gated.

## Visual Quality
Editor tools should be clean and pleasant to use, but functionality and reliability come before decorative complexity.
