---
trigger: model_decision
description: "Apply when packages, Package Manager, manifest.json, third-party libraries, DOTween Pro, asset-pack code, or package versions are involved."
---

# Package and Third-Party Rules

Before adding any package:
1. inspect `Packages/manifest.json`,
2. inspect existing imported/package functionality,
3. verify an equivalent is not already present,
4. explain why the dependency is needed.

Do not:
- modify package versions casually,
- upgrade packages during unrelated tasks,
- remove packages without explicit request,
- edit `PackageCache`,
- introduce a second tween/feedback/framework package because it is convenient.

Known approved creative dependencies/assets may include imported project packs such as:
- DOTween Pro,
- Epic Toon assets,
- POLYGON/stylized particle/VFX assets,
- café/environment/character packs already imported by the owner.

Use existing approved assets as a capability advantage.
Project-specific modifications/adapters belong under `Assets/Mannan`.

If package API/source behavior must be inspected, inspect it read-only first.
