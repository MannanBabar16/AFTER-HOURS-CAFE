---
trigger: model_decision
description: "Apply whenever Git status, diff, branches, commits, pushes, restores, resets, cleanup, or source-control operations are involved."
---

# Git Safety Rules

- Inspect `git status` before significant source-control operations.
- Never run destructive Git commands without explicit approval.
- Never run automatically:
  - `git reset --hard`
  - `git clean -fd`
  - `git clean -fdx`
  - `git push --force`
  - `git push --force-with-lease`
  - broad `git checkout -- .`
  - broad `git restore .`
  - branch deletion
- Do not rewrite history unless explicitly requested.
- Do not commit automatically unless asked.
- Do not push automatically unless asked.
- Do not casually modify `.gitignore`.
- Keep changes focused.
- Before finishing a coding task, summarize changed files and notable diff scope.
