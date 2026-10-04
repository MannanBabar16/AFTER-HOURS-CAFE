---
name: implement-unity-system
description: "Implements or extends a modular Unity gameplay system for After Hours Café, including architecture inspection, authoring data, runtime logic, validation, presentation hooks, testing, and safe integration."
---

# Implement Unity System

Use this skill for substantial gameplay-system implementation or extension.

## 1. Establish Intent
Identify:
- player-facing outcome,
- system responsibility,
- required dependencies,
- explicit non-goals,
- MVP acceptance criteria.

Consult the GDD and `Docs/ARCHITECTURE.md`.

## 2. Inspect Before Creating
Inspect:
- existing `Assets/Mannan` code/content,
- related definitions/prefabs,
- existing utilities,
- imported package capabilities,
- relevant Unity scene/project state.

Do not create a duplicate system because its existence was not obvious.

## 3. Define Ownership
Decide:
- definition data owner,
- runtime-state owner,
- prefab/view owner,
- save-facing state,
- public API,
- meaningful events.

Keep mutable runtime state out of definition ScriptableObjects.

## 4. Implement the Smallest Vertical Slice
Prefer a working narrow slice over a broad half-built framework.

Typical order:
1. pure logic/data,
2. runtime component/controller,
3. prefab/scene adapter,
4. presentation hooks,
5. authoring/validation support,
6. tests/debug visibility.

## 5. Use the Asset Toolbox
If the feature needs VFX/art/presentation:
- inspect imported assets,
- use suitable source content,
- duplicate/variant vendor assets before modifications,
- keep derivatives under `Assets/Mannan`,
- combine effects only when coherent and performant.

Never silently choose an ambiguous artistic asset when user selection is expected.

## 6. Add Feel
For player-facing interactions, consider:
- audio,
- VFX,
- animation,
- DOTween,
- UI,
- subtle camera/controller feedback.

Keep core logic independent from presentation.

## 7. Authoring Experience
If the feature will create repeated content or common errors, add a focused editor/validation tool rather than forcing repetitive inspector work.

Do not build a generic editor framework without a real need.

## 8. Validate
When possible:
- compile,
- inspect console,
- run targeted tests,
- play/smoke test the feature,
- inspect asset references,
- profile if making performance claims.

## 9. Report
Summarize:
- files/assets created or changed,
- architecture decisions,
- explicit references left for the user to assign,
- validation performed,
- known limitations,
- recommended next smallest step.

Do not auto-commit or push.
