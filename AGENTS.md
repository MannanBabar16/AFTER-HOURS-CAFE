# After Hours Café — Workspace Rules

## Project Identity
- Working title: **After Hours Café**.
- Unity project-owned root: `Assets/Mannan`.
- All new project-owned scripts, ScriptableObjects, prefabs, prefab variants, materials, textures, VFX, UI, editor tools, tests, settings, and generated content must live under `Assets/Mannan` unless a Unity-required location is unavoidable.
- Third-party/vendor packages stay in their original imported locations. Do not reorganize vendor folders merely for neatness.

## Product Goal
Build a small, polished PC café game where a few excellent modular systems combine into a cohesive experience. Prioritize:
1. satisfying interaction feel,
2. cozy presentation,
3. clean modular architecture,
4. reliable authoring workflows,
5. stable 60 FPS-class performance,
6. fast iteration for a solo developer.

## Engineering Philosophy
- Prefer the simplest production-ready design that satisfies the current feature.
- Favor composition over deep inheritance.
- Prefer small focused components over mega-managers.
- Do not introduce architecture for architecture's sake.
- Do not add a dependency-injection framework, generic service locator, global event bus, ECS conversion, or similar framework unless explicitly approved.
- Use explicit references and narrow interfaces. Use scoped C# events when decoupling is genuinely useful.
- Keep gameplay logic testable outside MonoBehaviour where practical.
- Use state machines only where state transitions are real and useful, such as customers, orders, machines, and shift flow.
- A system is not complete merely because it functions; it must expose clean presentation hooks so audio, VFX, animation, UI, and feedback can be layered without embedding them into core logic.

## Data and Runtime Authority
- **ScriptableObjects define authoring/configuration data.**
- **Prefabs define authored scene entities and presentation.**
- **Plain serializable runtime data owns mutable runtime/save state.**
- Never store current money, current relationship values, live orders, current inventory counts, or other mutable play-session state directly in shared definition ScriptableObjects.
- Prefer stable string/serialized IDs for save-facing definitions. IDs must be unique and validated.
- Do not load project content by arbitrary string paths at runtime when a serialized reference or registry is more appropriate.

## Prefab and Asset Authority
- User-authored prefab/model/material assignments are authoritative.
- Never silently replace an assigned asset because another asset appears more suitable.
- Never auto-assign an arbitrary model, prefab, material, icon, animation, or audio clip merely because its filename looks relevant.
- If a required artistic reference is missing, leave it visibly unassigned, produce a validation warning, and report what the user needs to assign.
- If the user explicitly names or selects an asset, that selection is authoritative.

## Imported Assets Are a Creative Superpower
- Before creating VFX, materials, props, environment pieces, animations, or presentation from scratch, inspect the already imported asset packs for reusable high-quality pieces.
- Actively use imported packages to increase visual quality and development speed.
- If an imported asset works as-is, reference it directly where safe.
- If an imported asset must be changed, **never destructively edit the vendor original**. Create a duplicate, material copy, prefab variant, derived prefab, or project-owned composition under `Assets/Mannan`, then modify the project-owned version.
- Imported VFX may be combined and adapted to create richer results when performance and visual coherence remain acceptable.
- Prefer one coherent, polished effect over stacking many unrelated effects.

## Third-Party Safety
- Do not modify files inside `PackageCache`.
- Do not modify vendor source/assets in place unless explicitly requested.
- Do not update, remove, or install packages as an unrelated side effect.
- Project-specific adapters/wrappers belong under `Assets/Mannan`.

## Search and Tool Discipline
- Inspect before inventing.
- Search for existing systems, definitions, prefabs, utilities, and package capabilities before creating duplicates.
- Use Antigravity's native file/code search and Unity-aware inspection tools first.
- Do not create or run Python, PowerShell, shell, or temporary scripts merely to search filenames, inspect ordinary source files, or locate assets.
- Use terminal search commands only when native project search is insufficient and the task justifies them.
- Use scripting languages only when the requested task genuinely requires processing/automation that cannot reasonably be done with normal project tools.
- Never use arbitrary execution as a substitute for understanding the project.

## Validation
After code or serialized-project changes when possible:
1. compile/reload scripts,
2. inspect Unity console,
3. validate the specific feature,
4. run relevant tests,
5. inspect changed files/diff,
6. report what was actually verified.
Never claim a feature works if it was not validated.

## Git Safety
- Git is the project safety net.
- Never run destructive Git commands without explicit approval.
- Never auto-commit or auto-push unless explicitly asked.
- Keep changes focused and report changed files.

## Scope Discipline
Before expanding a task:
- check the GDD and architecture documents,
- implement the smallest useful vertical slice,
- do not implement adjacent systems "while already here" unless they are required dependencies,
- record optional improvements instead of silently expanding scope.

## Project Documentation
Consult these when relevant:
- `Docs/ARCHITECTURE.md`
- `Docs/GDD_TECHNICAL_APPENDIX.md`
- the main Game Design Document supplied by the project owner.

When design intent and implementation convenience conflict, preserve design intent and report the tradeoff.
