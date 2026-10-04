# After Hours Café — Antigravity Setup Kit

## Where to Put This
Copy the contents of this kit into the **Unity project root** — the folder that contains:

```text
Assets/
Packages/
ProjectSettings/
```

After copying, the root should look approximately like:

```text
YourUnityProject/
├── AGENTS.md
├── .agents/
│   ├── rules/
│   └── skills/
├── Docs/
├── Assets/
├── Packages/
└── ProjectSettings/
```

Antigravity discovers workspace rules under `.agents/rules/*.md` and workspace skills under `.agents/skills/<skill>/SKILL.md`.

## Important
Do not copy `.agents` inside `Assets/`.
It belongs at the repository/workspace root.

`Assets/Mannan` is the root for **Unity project-owned content**, not for Antigravity configuration.

## Before the First Prompt
1. Open the Unity project and confirm imported packages/assets compile.
2. Initialize Git if needed.
3. Make a clean baseline commit yourself.
4. Copy this kit to the project root.
5. Put the main GDD in `Docs/` too, preferably as Markdown in addition to PDF/DOCX.
6. Open Antigravity at the Unity project root.
7. Start with `Docs/ANTIGRAVITY_FIRST_PROMPT.md`.

## Recommended Working Rhythm
For each milestone:
1. Ask Antigravity for one narrowly scoped implementation.
2. Let it inspect before editing.
3. Make it validate compilation/console/tests.
4. Review the diff and Unity scene yourself.
5. Play the feature.
6. Report the result to your project/design reviewer.
7. Decide the next smallest task.

## Key Principle
Antigravity is the implementation engineer inside the project.
The developer retains product, art, scope, and final architecture authority.
