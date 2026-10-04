---
trigger: model_decision
description: "Apply when implementing player-facing interactions, coffee preparation, serving, cleaning, rewards, UI feedback, transitions, VFX, audio cues, camera feedback, DOTween animation, or general game feel."
---

# Juice and Presentation Rules

Functional is not finished.

## Feedback Stack
For meaningful player actions, consider the appropriate combination of:
- animation,
- audio,
- VFX,
- DOTween motion,
- scale/punch,
- lighting,
- UI response,
- camera feedback,
- controller rumble.

Do not automatically use every channel.

## Feedback Intensity
Use three rough tiers:

### Micro
For frequent actions:
- tiny scale/punch,
- soft click/clink,
- subtle highlight,
- tiny particle response.

### Standard
For satisfying actions:
- stronger sound,
- clear motion,
- brief VFX,
- UI confirmation,
- light camera/controller feedback if appropriate.

### Highlight
For rare accomplishments:
- richer layered effects,
- musical accent,
- larger UI presentation,
- stronger but short camera/lighting feedback.

Constant highlight-level feedback destroys contrast.

## Core/Presentation Separation
Core gameplay should expose meaningful hooks.
Example:
- extraction started,
- extraction progress,
- extraction completed.

Presentation components may subscribe/react:
- coffee machine audio,
- steam VFX,
- animation,
- UI,
- feedback.

Do not put unrelated camera/UI/VFX logic directly inside domain logic.

## DOTween Pro
Use DOTween when it simplifies:
- UI motion/fades/scales,
- small object presentation,
- punch/shake,
- short transitions,
- menu/phone animation,
- reward feedback.

Rules:
- never start tweens every frame,
- prevent uncontrolled overlap,
- kill/link tweens correctly on disable/destroy,
- keep sequences readable,
- do not replace physics motion with tweens when physics is authoritative,
- do not replace Animator when authored animation is the better tool.

## Camera
Camera feedback must be subtle in a cozy game.
Avoid:
- constant shake,
- nausea-inducing movement,
- FOV spam.

Use stronger camera feedback only for genuinely important events.

## Audio
Audio is a first-class feedback layer:
- ceramic clink,
- portafilter lock,
- grinder,
- steam,
- pour,
- tray placement,
- door chime,
- rain transition,
- robot tones.

Prefer timing accuracy and variation over loudness.

## Reusability
Create reusable feedback prefabs/components for repeated interaction families, but do not build a giant generic feedback framework before actual repetition exists.
