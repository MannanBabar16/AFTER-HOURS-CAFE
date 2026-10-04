---
trigger: model_decision
description: "Apply when implementing or reviewing runtime gameplay, AI, physics, rendering, UI, VFX, spawning, pooling, memory, or performance-sensitive systems for the PC build."
---

# PC Performance Rules

## Target
Primary baseline: a smooth **60 FPS at 1080p on a reasonable mid-range gaming PC**, while keeping scalable quality options practical.

60 FPS provides ~16.67 ms total frame time. Treat this as a system-wide budget, not permission for one feature to consume it.

## Philosophy
- Profile before complex optimization.
- Do not prematurely micro-optimize.
- Do not knowingly build obvious hot-path inefficiencies.
- Prefer the simplest performant implementation.

## Hot Paths
Avoid unnecessary work in:
- `Update`
- `FixedUpdate`
- `LateUpdate`
- high-frequency coroutines
- UI rebuild paths
- particle callbacks
- repeated AI polling

Avoid:
- per-frame allocations,
- string building in hot paths,
- LINQ in high-frequency loops,
- repeated scene-wide searches,
- repeated `GetComponents` calls,
- frequent material instancing,
- unnecessary physics queries.

## Event-Driven / Coarse Updates
Prefer event-driven recalculation when state changes.
Examples:
- café style score recalculates when furniture changes,
- seating availability updates when seat occupancy changes,
- order state advances on actions/events,
- social simulation runs on meaningful events or coarse ticks.

Customer planning/needs do not need full reasoning every rendered frame.

## Pooling
Use pooling when lifecycle frequency justifies it:
- repeated particles,
- temporary UI feedback,
- frequently spawned transient objects,
- repeated effect prefabs.

Do not build a universal pooling framework for static café content.

## Physics
- Keep colliders simple where possible.
- Use appropriate layer collision matrices.
- Avoid expensive broad queries every frame.
- Use physics for interactions that genuinely benefit from it; do not simulate everything physically for appearance alone.

## Rendering / VFX
- Prefer shared materials and batching-friendly setups.
- Use GPU instancing/static batching where suitable.
- Limit transparency overdraw and excessive real-time lights.
- Optimize copied asset-pack VFX for the actual use case.
- Avoid stacking expensive post effects for ordinary interactions.

## UI
- Separate highly dynamic UI from mostly static UI when this reduces Canvas rebuild cost.
- Avoid constantly rebuilding large hierarchies.
- Pool repeated notification/feed items if churn becomes meaningful.
- Prefer atlased sprites for runtime UI.

## Memory / Loading
- Do not duplicate large textures/materials accidentally.
- Keep derived assets intentional.
- Avoid loading huge content sets when only a small subset is needed.
- Add Addressables or streaming architecture only if project scale actually requires it.

## Profiling
For performance-sensitive milestones, inspect:
- CPU frame,
- GPU frame,
- GC allocations,
- render batches/setpass,
- physics cost,
- UI rebuild cost,
- VFX overdraw when relevant.

Optimization claims must be supported by profiling, not guesses.
