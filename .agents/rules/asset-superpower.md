---
trigger: model_decision
description: "Apply whenever using, choosing, adapting, combining, or authoring VFX, particles, materials, models, props, animations, audio-visual feedback, UI presentation, or third-party asset-pack content."
---

# Imported Asset Superpower Rules

The project owner intentionally imported quality asset packs so the agent can use them aggressively as a creative toolbox.

## Default Workflow
When a feature needs visual/audio presentation:
1. Inspect relevant imported assets first.
2. Identify promising prefabs, particle systems, materials, meshes, animations, textures, icons, or utility components.
3. Decide whether the source can be referenced safely as-is.
4. If modification is needed, create a project-owned derivative under `Assets/Mannan`.
5. Modify only the derivative.
6. Compose multiple effects only when the result is coherent and performant.
7. Validate appearance in context, not only in isolation.

## Derived Asset Location
Place modified derivatives under a clear project-owned location, for example:
- `Assets/Mannan/Derived/<SourcePack>/VFX/`
- `Assets/Mannan/Derived/<SourcePack>/Materials/`
- `Assets/Mannan/Derived/<SourcePack>/Prefabs/`
- `Assets/Mannan/Derived/<SourcePack>/Textures/`

Feature-specific final prefabs may then live under normal `Assets/Mannan/Prefabs/...` folders.

## Vendor Preservation
Never:
- overwrite vendor originals,
- edit package source in place,
- rename/move vendor folders,
- globally change shared vendor materials just to fix one project prefab.

Prefer:
- prefab variants,
- duplicated project-owned materials,
- child/composition prefabs,
- duplicated particle prefabs,
- project-owned animation/controller overrides.

## Creative Use
Imported assets may be:
- recolored,
- rescaled,
- retimed,
- remixed,
- layered,
- re-emitted,
- combined with DOTween,
- paired with audio,
- paired with light flashes,
- paired with camera feedback,
- converted into reusable project-specific feedback prefabs.

Example:
A coffee spill may combine a duplicated/adapted stylized liquid particle with a subtle impact puff, sound, decal/mesh, and a small tweened cup reaction.

## Cohesion
More effects are not automatically better.
Every effect must support:
- the cozy stylized art direction,
- readability,
- action timing,
- perceived physicality,
- performance.

Avoid visual noise, excessive bloom, constant screen shake, or effects that compete with the coffee/café atmosphere.

## Asset Selection Authority
The agent may inspect and recommend assets.
It must not silently assign an arbitrary artistic asset if the user has not authorized automatic selection for that feature.
If selection is ambiguous, create the technical slot/reference and report candidate assets instead of guessing.

## Performance
For reused VFX:
- control particle count and lifetime,
- disable unnecessary lights/trails/collision,
- pool frequently spawned effects,
- avoid heavy transparent overdraw,
- avoid unnecessary material instances,
- check effect cost in the actual café scene.
