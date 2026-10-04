---
trigger: glob
globs: "Assets/Mannan/**/*.cs"
description: "Unity C# coding conventions for project-owned runtime and editor code."
---

# Unity C# Coding Rules

- Prefer simple, readable, production-friendly C#.
- Use `[SerializeField] private` fields for inspector references unless public API access is genuinely required.
- Preserve encapsulation; do not make fields public for inspector convenience.
- Prefer readonly properties for exposing state.
- Cache frequently used Unity references.
- Avoid repeated `GameObject.Find`, `FindFirstObjectByType`, `FindAnyObjectByType`, `GetComponent`, `Camera.main`, or hierarchy traversal in hot paths when references can be assigned/cached.
- Avoid LINQ and avoidable allocations in frequent gameplay loops.
- Do not use `async void` except Unity/event signatures that genuinely require it.
- Do not create a MonoBehaviour when a plain C# class or ScriptableObject definition is more appropriate.
- Runtime assemblies must never reference `UnityEditor`.
- Editor-only code must be in an Editor-only assembly/folder.
- Follow the `Mannan.*` namespace convention.
- Before renaming serialized fields, preserve data with `FormerlySerializedAs` where appropriate.
- Search usages before changing public APIs, class names, serialized fields, IDs, or namespaces.
- Do not silently remove behavior used elsewhere.
- Favor explicit methods and types over clever metaprogramming.
- Add comments for why, invariants, or non-obvious constraints—not for obvious line-by-line narration.
- Keep method/class size reasonable; extract only when it improves responsibility/readability.
- Prefer deterministic, testable calculations for domain logic.
- Use Unity's serialization intentionally. Do not rely on unsupported polymorphic serialization without a clear need.
- Use `Try...` APIs where failure is expected and should not throw.
