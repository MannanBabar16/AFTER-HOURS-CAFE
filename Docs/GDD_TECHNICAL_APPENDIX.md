# After Hours Café — GDD Technical Production Appendix

This appendix supplements the main Game Design Document with production decisions.

## Locked Technical Decisions

| Area | Decision |
|---|---|
| Project-owned root | `Assets/Mannan` |
| Render pipeline | Use the project's current URP setup; do not migrate render pipeline casually |
| Runtime UI | uGUI + TextMeshPro |
| UI textures | Sprite Atlas where appropriate |
| Editor UI | UI Toolkit for substantial tools |
| Tween/presentation | DOTween Pro |
| VFX strategy | Reuse/adapt imported stylized asset-pack VFX plus project-owned derivatives |
| Definitions | ScriptableObjects |
| Mutable runtime state | Plain serializable C# runtime/save data |
| Scene entities | Prefabs / prefab variants |
| Vendor assets | Read/reference by default; duplicate/variant before modification |
| Art assignment | Explicit/user-authored; do not silently guess missing prefabs/models |
| Architecture | Modular composition |
| Communication | Explicit references + narrow interfaces + scoped events |
| Global event bus | No |
| DI framework | No initially |
| Runtime string-path content lookup | Avoid as primary architecture |
| Project search | Native Antigravity/Unity search first; no Python for ordinary searches |
| Performance baseline | Smooth 60 FPS @ 1080p on a reasonable mid-range PC |
| Source control | Git baseline, focused changes, no auto commit/push |
| Editor tooling | First-class project feature |
| Save system | Versioned |
| Primary platform | PC / Steam |
| Camera & View | 3/4 isometric-inspired perspective diorama camera; direct camera-relative WASD control; contextual close-up workstation cameras. No first-person or over-the-shoulder gameplay. |
| Input | Keyboard/mouse baseline (WASD + E/Q); keep controller support feasible |

## Imported Asset Philosophy
Imported assets are a deliberate production advantage.

The implementation agent should:
- search imported packs before recreating presentation,
- reuse quality assets as-is when appropriate,
- duplicate or create variants under `Assets/Mannan` before modification,
- combine compatible VFX/animation/audio/tween layers for stronger game feel,
- keep results stylistically coherent and performant,
- never damage vendor originals.

## Authoring Philosophy
The developer retains artistic control.

When a feature needs a model/prefab/icon/material:
- use an explicitly assigned reference when one exists,
- do not replace it silently,
- if missing and artistic choice matters, expose the slot and validation warning,
- recommendation is allowed; silent guessing is not.

## Editor Tooling Requirement
When a content-heavy system is implemented, evaluate whether a small editor tool can:
- accelerate repeated authoring,
- validate IDs/references,
- reduce inspector mistakes,
- speed up playtesting.

Do not build generic tools before repetition exists.

## Juice Requirement
Player-facing systems are not considered polished merely because functional behavior is complete. They should expose clean hooks for:
- audio,
- VFX,
- animation,
- DOTween,
- UI,
- subtle camera/controller feedback.

Use feedback proportionally so the cozy tone remains readable.

## Performance Requirement
Avoid architectural hot-path waste from the start, but profile before complex optimization.

Key principles:
- event-driven updates,
- coarse AI planning,
- controlled physics,
- controlled particle overdraw,
- pooled repeated transient feedback,
- efficient dynamic UI,
- shared materials where practical.

## Modular Completion Definition
A substantial system should ultimately provide:
1. clear responsibility,
2. authoring definitions,
3. runtime implementation,
4. integration API/events,
5. prefab/view integration,
6. validation/debug visibility,
7. presentation hooks,
8. targeted testing/verification.
