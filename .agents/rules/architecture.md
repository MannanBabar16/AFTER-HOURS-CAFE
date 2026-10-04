---
trigger: model_decision
description: "Apply when designing, adding, refactoring, or reviewing gameplay systems, system boundaries, data ownership, dependencies, or module architecture for After Hours Café."
---

# Modular Architecture Rules

## Core Direction
The game is built from small modules that work independently and integrate through narrow contracts.

Preferred module families:
- Core
- Interaction
- Coffee
- Orders
- Customers
- Cafe
- Economy
- Inventory
- Social
- Progression
- Robots
- World
- Save
- UI
- Audio/Feedback adapters
- Editor tooling

## Responsibilities
Each system should have:
- one clear responsibility,
- an explicit owner of mutable state,
- explicit inputs and outputs,
- a small public API,
- predictable lifecycle,
- validation for required references/data.

Avoid systems that become catch-all coordinators.

## Composition
Prefer:
- small MonoBehaviours attached to authored prefabs,
- plain C# domain/runtime classes,
- ScriptableObject definitions,
- narrow interfaces for substitutable behavior,
- direct serialized references for local dependencies,
- scoped events for cross-module notifications.

Avoid:
- deep inheritance trees,
- static mutable global state,
- scene-wide lookups as architecture,
- generic "GameManager" objects that own unrelated concerns,
- global event buses for every interaction,
- reflection-heavy registration.

## Composition Root
A small bootstrap/composition layer may create or connect long-lived services. It must not become a god object.

Long-lived services should exist only when lifecycle and ownership justify them, for example:
- save/profile service,
- shift/session coordinator,
- definition registries,
- audio routing.

## ScriptableObjects
Use ScriptableObjects for definitions and authoring:
- recipe definitions,
- ingredient definitions,
- customer definitions,
- furniture definitions,
- events,
- upgrades,
- music,
- social templates.

Do not use ScriptableObjects as hidden mutable global runtime singletons.

## Runtime State
Mutable runtime data should be plain serializable C# data where possible:
- `CafeRuntimeState`
- `CustomerRuntimeState`
- `OrderRuntimeState`
- `InventoryRuntimeState`
- `ProgressionSaveData`

Keep definition references separate from mutable state.

## Events
Events should describe meaningful state changes:
- `OrderCreated`
- `OrderCompleted`
- `DrinkPrepared`
- `RelationshipChanged`
- `ShiftStarted`
- `ShiftEnded`
- `CafeStyleChanged`

Do not emit events every frame or for information that can be queried directly.

## State Machines
Use explicit state machines where behavior has real states and transitions:
- customer lifecycle,
- order lifecycle,
- coffee machine operations,
- shift lifecycle,
- robot task lifecycle.

Do not create a state machine for trivial binary flags.

## Dependencies
A feature should depend on abstractions only when substitution/testing/decoupling provides real value.
Do not create interfaces for every class by default.

## System Completion
A new module should ideally provide:
- runtime implementation,
- authoring data,
- validation,
- debug visibility,
- minimal tests for logic-heavy code,
- clean presentation hooks.
