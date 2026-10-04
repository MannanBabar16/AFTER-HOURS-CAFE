# First Antigravity Prompt — Project Foundation

Paste this into Antigravity after this setup kit is copied to the Unity project root and the current project has a clean Git baseline.

---

We are beginning development of **After Hours Café**.

Before editing anything:
1. Read the root `AGENTS.md`.
2. Read relevant `.agents/rules`.
3. Read `Docs/ARCHITECTURE.md`.
4. Read `Docs/GDD_TECHNICAL_APPENDIX.md`.
5. Inspect the current Unity project, package manifest, imported assets, existing folders, render pipeline, input setup, and compilation/console state.
6. Do not use Python or temporary scripts for ordinary project searching.
7. Do not modify vendor/third-party assets in place.

## Task: P0 Project Foundation Only

Establish the clean project foundation under `Assets/Mannan`.

Implement only what is necessary for a maintainable modular baseline. Do **not** implement coffee gameplay, customers, social media, economy gameplay, robots, progression content, or the full café loop yet.

### Required Outcomes
- Create the agreed `Assets/Mannan` project structure where useful.
- Establish minimal runtime/editor/test assembly definitions if they are appropriate for the current project and will not conflict with imported assets.
- Establish `Mannan.*` namespaces.
- Create a small, non-god-object project bootstrap/composition foundation only if needed.
- Create the first project validation foundation/editor entry point.
- Create a development/debug foundation only to the extent needed for future system testing; do not overbuild it.
- Preserve the project's current URP/package/imported-asset setup.
- Do not install new packages.
- Do not auto-select arbitrary art assets.
- Keep artistic prefab/model assignments under developer control.
- Ensure the architecture can later support ScriptableObject definitions + plain runtime state + prefab views.

### Editor Tool Direction
Create a clean starting menu root:
`Mannan/After Hours Café`

If creating an initial project window/validator is reasonable in this foundation step, keep it small and production-ready. Do not build the entire future editor suite yet.

### Quality Constraints
- Simple code.
- Modular boundaries.
- No global event bus.
- No DI framework.
- No giant manager.
- No static mutable gameplay state.
- No runtime `Resources.Load` architecture.
- No premature generic frameworks.
- No modifications to vendor originals.

### Validation
After implementation:
- compile/reload Unity,
- check console,
- run any relevant tests,
- inspect the final changed-file list,
- report any intentionally unassigned references.

### Final Report
Return:
1. architecture created,
2. every file created/modified,
3. what was validated,
4. any risks/assumptions,
5. the exact smallest recommended next task.

Do not commit or push.
