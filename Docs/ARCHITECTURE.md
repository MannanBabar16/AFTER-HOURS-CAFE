# After Hours Café — Technical Architecture

## 1. Purpose
This document defines the implementation shape of the project. The GDD defines what the game should feel like and contain; this document defines how systems should be separated and integrated.

## 2. Project-Owned Root
All project-owned content lives under:

`Assets/Mannan`

Recommended structure:

```text
Assets/Mannan/
├── Art/
├── Audio/
├── Data/
├── Derived/
├── Prefabs/
├── Scenes/
├── Scripts/
│   ├── Runtime/
│   │   ├── Core/
│   │   ├── Interaction/
│   │   ├── Coffee/
│   │   ├── Orders/
│   │   ├── Customers/
│   │   ├── Cafe/
│   │   ├── Economy/
│   │   ├── Inventory/
│   │   ├── Social/
│   │   ├── Progression/
│   │   ├── Robots/
│   │   ├── World/
│   │   ├── Save/
│   │   └── UI/
│   └── Editor/
│       ├── Tools/
│       ├── Inspectors/
│       └── Validation/
├── UI/
│   ├── Sprites/
│   ├── Atlases/
│   ├── Fonts/
│   └── Icons/
├── Settings/
└── Tests/
    ├── EditMode/
    └── PlayMode/
```

Do not move third-party imported packs into this folder. Project-owned derivatives of vendor assets belong under `Assets/Mannan/Derived`.

## 3. Assemblies
Start simple:
- `Mannan.Runtime`
- `Mannan.Editor`
- `Mannan.Tests`

Split further only when compilation/dependency boundaries provide real value.

## 4. Namespace
Use `Mannan.*`, for example:
- `Mannan.Core`
- `Mannan.Coffee`
- `Mannan.Customers`
- `Mannan.Social`
- `Mannan.Editor`

## 5. Architectural Layers

### Definition Layer
ScriptableObjects define authored content:
- recipes,
- ingredients,
- customers,
- furniture,
- upgrades,
- events,
- social templates.

Definitions are effectively immutable during gameplay.

### Runtime Domain Layer
Plain C# state/logic where practical:
- orders,
- economy calculations,
- progression,
- customer familiarity,
- session metrics,
- save-facing state.

### Unity/Presentation Layer
MonoBehaviours and prefabs:
- scene interactions,
- animation,
- NavMesh adapters,
- particles,
- audio emitters,
- UI views,
- authored sockets/references.

### Editor Layer
Authoring tools, validators, debug/playtest tools.

## 6. System Communication
Priority:
1. direct serialized reference for local prefab relationships,
2. direct constructor/setup reference for plain runtime objects,
3. narrow interface where substitution/decoupling matters,
4. scoped C# event for meaningful cross-module notifications.

Avoid a global event bus.

## 7. Composition
A small bootstrap may connect long-lived systems. It should not contain feature logic.

Examples of legitimate long-lived systems:
- save/profile service,
- shift/session coordinator,
- audio routing,
- content/definition registry if needed.

## 8. Core System Families

### Interaction
Generic first-person interaction contract:
- focus/eligibility,
- prompt,
- interact,
- hold/progress when required,
- hand/carried-object compatibility.

Keep it generic enough for coffee tools, trays, cleaning, doors, phones, etc., without making it a universal action framework.

### Coffee
Owns:
- recipe definitions,
- coffee preparation steps,
- machine/tool states,
- drink result/evaluation.

Presentation reacts to coffee operation hooks.

### Orders
Owns:
- requested items,
- status,
- timing,
- completion evaluation,
- payment/tip result inputs.

### Customers
Owns:
- customer definitions,
- runtime customer state,
- visit lifecycle,
- preferences,
- familiarity/regular progression,
- activity selection,
- seating/navigation coordination.

### Cafe
Owns:
- furniture/placeables,
- seating registration,
- style-tag aggregation,
- cleanliness state where applicable,
- café-level configuration.

### Economy
Owns:
- balance,
- transactions,
- revenue/expense records,
- pricing inputs.

### Inventory
Owns stock counts and restocking. Keep it light; avoid warehouse simulation.

### Social
Owns:
- customer-post generation rules,
- feed entries,
- follower/reach simulation,
- reactions driven by actual café events.

Use procedural/templates first; generative AI is not required for v1.

### Progression
Owns:
- unlock conditions,
- reputation,
- ownership/buy-the-café campaign progress,
- collection progress.

### Robots
Support chores; do not automate the core coffee-making fantasy.
Examples:
- cleaner robot,
- delivery/helper robot later.

### World
Owns:
- shift time,
- weather state,
- seasonal ambience,
- neighborhood state,
- rare-night/event coordination.

### Save
Owns versioned persistent state and migration/default handling.

### UI
Displays and requests actions. It does not own core business rules.

## 9. Prefab/Data Control
Artistic references remain explicitly authored by the developer.
Antigravity may create slots, validators, project-owned variants, and candidate recommendations, but should not silently choose arbitrary art.

## 10. Presentation Architecture
Gameplay modules expose meaningful hooks:
- start,
- progress,
- complete,
- fail/cancel,
- state changed.

Separate adapters handle:
- audio,
- VFX,
- DOTween,
- UI,
- camera/controller response.

## 11. Editor Architecture
Editor tooling grows with system repetition.

Priority tools:
1. Project Validator
2. Coffee Recipe Editor
3. Customer Authoring Browser
4. Playtest/Debug Panel
5. Furniture/Style Database
6. Event/Social authoring tools

Long-term central window:
`Mannan > After Hours Café`

## 12. Performance Direction
- 60 FPS @ 1080p mid-range PC baseline.
- Event-driven/coarse updates over per-frame simulation.
- Pool repeated transient effects when justified.
- Keep physics and transparent VFX controlled.
- Profile before complex optimization.

## 13. Runtime UI
Default:
- uGUI,
- TextMeshPro,
- Sprite Atlas,
- DOTween Pro.

Editor tooling:
- UI Toolkit.

## 14. Explicit Non-Goals for Foundation
Do not add during foundation unless requested:
- multiplayer,
- ECS conversion,
- Addressables migration,
- dependency-injection framework,
- generic global event bus,
- procedural city generation,
- generative-AI NPC dialogue,
- massive inventory/employee systems.
