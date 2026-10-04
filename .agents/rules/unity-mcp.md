---
trigger: model_decision
description: "Apply when using Unity MCP/editor automation or any tool that can inspect or mutate the running Unity Editor, scenes, prefabs, settings, or assets."
---

# Unity Editor Automation Safety

When Unity-aware tooling/MCP is available, prefer direct Unity inspection over assumptions.

## Read First
Before mutation:
1. inspect target asset/object,
2. confirm ownership (vendor vs Mannan),
3. inspect serialized references/current values,
4. prefer dry-run/preview where available,
5. make the smallest targeted change.

## Safer Read Operations
Prefer read-only inspection for:
- editor status,
- open scenes,
- hierarchy,
- selection,
- asset search,
- component properties,
- serialized fields,
- console,
- build/player/quality/graphics/physics settings.

## Ask Before Broad/Destructive Work
Do not perform broad destructive actions without explicit approval:
- delete assets/GameObjects,
- clear baked data,
- remove packages,
- overwrite existing user assets,
- mass-edit large hierarchies,
- change build target,
- change major project/global settings.

## Dedicated Operations First
Prefer dedicated Unity/editor operations over arbitrary C# evaluation or script execution.
Use arbitrary execution only when a dedicated operation cannot reasonably perform the task.

## After Mutation
- compile/reload if needed,
- inspect console,
- verify target state,
- save only what is intentionally changed,
- report exact changes.
