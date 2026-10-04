---
trigger: model_decision
description: "Apply for non-trivial implementation, debugging, refactoring, integration, or Unity project modification tasks."
---

# Unity Agent Workflow

For non-trivial tasks:

1. Understand the requested outcome and explicit non-goals.
2. Read relevant project documentation/rules.
3. Inspect relevant existing project files and Unity state.
4. Search for an existing system/utility/asset before creating a new one.
5. Identify the smallest required change and dependencies.
6. Preserve explicit prefab/model/material assignments.
7. Implement the smallest useful vertical slice.
8. Add presentation hooks; use imported asset packs and DOTween where appropriate.
9. Add/editor tooling only when it removes real repetition or validation risk.
10. Compile/reload when code changes.
11. Check compilation status and Unity console.
12. Run relevant tests/feature validation when available.
13. Inspect changed files/diff.
14. Summarize:
    - what changed,
    - what was validated,
    - what remains intentionally unassigned,
    - any follow-up risks.

## Debugging
- Investigate the actual failing path first.
- Do not rewrite unrelated systems to fix one error.
- Reproduce when possible.
- Prefer root-cause fixes over symptom patches.
- Do not claim success without validation.

## Search
Do not use Python or temporary scripts for normal searching.
Prefer native project/code search and Unity-aware inspection.

## Scope
Do not silently add adjacent features.
Record useful future ideas separately.
