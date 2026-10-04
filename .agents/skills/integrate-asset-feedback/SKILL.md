---
name: integrate-asset-feedback
description: "Uses imported asset packs, DOTween Pro, audio, VFX, animation, and project-owned derivatives to create polished, performant interaction feedback without modifying vendor originals."
---

# Integrate Asset Feedback

Use when a feature is functionally correct but needs visual/sensory polish.

## 1. Identify the Action
Define:
- action start,
- important timing beats,
- successful completion,
- failure/cancel state,
- repetition frequency.

## 2. Inspect Existing Assets
Search imported packs for:
- particles/VFX,
- materials,
- meshes,
- animations,
- audio if available,
- related prefabs/components.

## 3. Preserve Vendor Assets
If modification is required:
- duplicate/variant into `Assets/Mannan/Derived/...`,
- rename meaningfully,
- modify the project-owned copy only.

## 4. Compose Feedback
Choose only the channels that help:
- animation,
- sound,
- particle/VFX,
- DOTween,
- light,
- UI,
- subtle camera/controller response.

Match feedback strength to action importance.

## 5. Optimize
Check:
- particle count,
- transparency/overdraw,
- light count,
- material duplication,
- spawn churn,
- tween lifecycle.

Pool frequently spawned feedback when justified.

## 6. Integrate Cleanly
Presentation should react to gameplay hooks/events rather than own gameplay decisions.

## 7. Verify In Context
Judge the effect inside the café scene and during repeated play, not only in the Scene view.
