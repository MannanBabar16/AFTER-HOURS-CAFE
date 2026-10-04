---
trigger: model_decision
description: "Apply when designing or implementing runtime UI, HUD, phone apps, menus, prompts, notifications, social feed, UI animation, sprite atlases, or editor UI."
---

# UI Rules

## Runtime UI Stack
Default runtime UI:
- Unity uGUI / Canvas,
- TextMeshPro,
- Sprite Atlases,
- DOTween Pro for presentation animation.

Do not introduce a second runtime UI stack without a specific reason.

## Editor UI Stack
Default custom editor tooling:
- UI Toolkit,
- UXML/USS where they improve maintainability,
- standard Unity editor APIs.

## Runtime Style
The UI should feel:
- clean,
- premium,
- cozy,
- readable,
- lightly animated,
- modern Korean-café inspired rather than generic mobile-game UI.

Avoid excessive:
- gradients,
- bouncing,
- popups,
- screen-covering rewards,
- attention competition.

## Phone
The in-game phone is an immersive management surface, not a replacement for every gameplay interaction.
Potential apps:
- CafeGram/social,
- bank/earnings,
- shop/catalog,
- café management,
- messages.

Keep app responsibilities distinct.

## Animation
Use short, responsive animations.
- input feedback should feel immediate,
- animation must not block routine actions,
- respect interruption/cancellation,
- avoid overlapping tween spam.

## Sprite Atlases
Group UI sprites intentionally to reduce texture switches.
Do not create one huge atlas containing unrelated high-resolution content if it harms memory/workflow.

## World-Space UI
Keep prompts minimal.
Prefer context-sensitive prompts over persistent icon clutter.

## Data
UI displays state; it should not own core gameplay state or business rules.

## Validation
Missing icons, references, localization keys, or invalid view bindings should be detectable by editor validation where practical.
