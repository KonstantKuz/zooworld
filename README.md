# Zoo World

**[Play the WebGL demo](https://konstantkuz.github.io/zooworld/)**

3D zoo seen from above (a perspective camera, slightly tilted): every 1-2 seconds an animal appears and wanders. Prey fly apart when they meet, predators eat whatever
they touch (a predator duel leaves one survivor), a "Tasty!" label follows a predator that has just eaten, a hungry predator carries "Hungry!" and a frightened prey "Scary!" for as long as the state lasts, and the
top-right counters show dead prey and dead predators.

Unity 6 (URP) · Zenject · R3 · UniTask · NavMesh (pathfinding only) · uGUI

## Run

Open the project in Unity 6 (6000.3), open `Assets/Scenes/Zoo.unity` and press Play. Everything needed is committed.

To regenerate the configs, prefabs and the scene from scratch, run **ZooWorld → Build Demo Scene** (it overwrites them).

## Layers

Code is split into three assemblies. Each one may use only what is listed, so the compiler enforces the direction.

| Layer | Assembly | Depends on | What lives here |
|---|---|---|---|
| **Core** | `Zoo.Core` | Unity and plugins only | Contracts and generic machinery that know nothing about this zoo or about how things move, look or collide: actor, factory and lifecycle, module and capability interfaces, interaction rules framework, behaviour tree composites (`Selector`, `Sequence`, `Move`, `Wait`), `IRoute`, `IMovementStrategy`, navigation interfaces, pooling |
| **Game** | `Zoo.Game` | Core | Every concrete implementation and rule of this project: modules on a Rigidbody (movement, visual, health, collision), gaits and routes, NavMesh and camera services, predator/prey traits and rules, `Hunt`/`Flee` nodes, spawning, kill statistics, level generation, installers |
| **UI** | `Zoo.UI` | Core, Game | Views: counters and the "Tasty!" label |

An animal is not code: it is an `ActorConfig` asset (a prefab plus traits and modules). `Zoo.Editor` (scene builder) sees all
layers and nothing sees it.

## Tests

Run them from Window > General > Test Runner (the EditMode and PlayMode tabs).

| Folder | Assembly | What is covered |
|---|---|---|
| `Assets/Tests/EditMode` | `Zoo.Tests.EditMode` | Plain logic with fakes, fast: behaviour tree nodes, routes and path following, interaction rules and the game rules played through the real collision pipeline, collision resolution, actor lifecycle, kill counters, statuses, level generation |
| `Assets/Tests/PlayMode` | `Zoo.Tests.PlayMode` | Things that need Unity running: pooling, physics perception, NavMesh paths and level connectivity, camera playfield and returning to the screen, hunger and fear timing, stopping movement on a Rigidbody, the "Tasty!" label, a smoke run of the `Zoo` scene |
| `Assets/Tests/Common` | `Zoo.Tests.Common` | Fakes shared by both: fake actors, movement, navigation, statuses |

Known issue (the test `EverySlotOfTheDefaultLevelGetsAWall` is ignored until it is fixed): with the default wall slots
about 8% of generated levels lose a wall, because two slots conflict at their home positions and the fallback placement is
rejected as well.

Config assets live in `Assets/Content/Configs`, grouped as `Actors`, `Traits`, `Modules`, `Movement`, `Behaviour/<Actor>`.

## Architecture in one picture

```
ActorConfig  (ScriptableObject)           what a species is
 ├─ traits   [PreyTrait | PredatorTrait]   "is a prey", "is a predator", "is a cat"...
 └─ modules  [Movement, Visual, Collision, Health, Behaviour...]   what it does
                      │  ActorFactory turns every module recipe into a live module
                      ▼
Actor  (the only MonoBehaviour, no subclasses)
 └─ forwards Unity callbacks (FixedUpdate, OnCollisionEnter) to the modules that want them
```

An animal has no species-specific code. A new species is a new `ActorConfig` asset: pick traits, list modules,
add it to the catalog. Nothing in the animal system changes (open/closed).

| Concept | Where | Pattern |
|---|---|---|
| Species as data | `Core/Actors/Configs` | Data-driven ScriptableObjects, Flyweight (shared trait/module assets) |
| Creating an animal | `ActorFactory` | **Factory**: takes an animal from the pool and assembles it from its config. The configs themselves are **Factory Methods**: `ActorModuleConfig`, `BehaviourConfig` and `MovementConfig` each `Create` what they describe |
| Behaviour pieces | `Core/Actors/Modules`, `Game/Actors/Modules` | **Composition over inheritance**, capability interfaces (`IMovement` (position and gaits), `IPushable`, `IHealth`, `ITickModule`, `ICollisionModule`); one module may implement several |
| How to move | `Core/Actors/Modules/Movement`, `Game/Actors/Modules/Movement` | **Strategy** (`LinearMovement`, `JumpMovement`) as gaits owned by the `MovementModule`, which also holds position and knockback and can stop the actor; where to go is a separate `IRoute` (wander / chase) |
| What to do | `Core/Actors/Modules/Behaviour`, `Game/Actors/Modules/Behaviour` | **Behaviour tree** assembled from configs (Selector, Sequence, Move, Wait, Hunt, Flee); `Wait` brings the actor to a stop through `IMovement.Stop`, every gait implements it |
| Who eats whom | `Core/Actors/Interactions`, `Game/Actors/Interactions` | **Rules** over traits: `InteractionRule<IPredator, IPrey>`, a chain of `IInteractionRule`. Contacts go into a buffer and are resolved on the next fixed step in a fixed order (by animal id), so the outcome never depends on which animal Unity reports first |
| Animal life | `ActorLifecycleService` | **Service**: spawn and kill through the factory; events through `IActorEvents` (R3 **Observer**) |
| Reuse of instances | `Pooling` | **Object pool**: prewarmed, capped, double-release guarded, released by instance (`PoolService` over `ComponentPool`); used by animals, walls and UI labels |
| Wiring | `Core/Installers`, `Game/Installers` | **Dependency injection** (Zenject), one static installer per feature (`Animals`, `Pooling`, `Navigation`, `Level`); everything depends on interfaces. Modules ask for the services they need in their own constructor, `ActorContext.Create<T>()` injects them |
| UI | `UI` | Views subscribe to services/events; "Tasty!" lives on the canvas and uses the pool |

## Adding an animal

1. Create an `ActorConfig`: prefab, mass, traits (`PreyTrait` or `PredatorTrait`, or a new trait asset).
2. List its modules: `MovementModule`, `CollisionModule`, `HealthModule`, and a `BehaviourModule` whose tree uses
   `Move` nodes with a gait (a movement config).
3. Add it to the `ActorCatalog`.

A brand new kind of behaviour is a new class plus its config (a module, a behaviour node, a movement strategy or an
interaction rule). A mushroom that never moves lists no behaviour module and a tiny `IMovement` module that only reports a position. An immortal animal lists no
`HealthModule`.

Examples in the demo:

- **Frog** (prey): jumps a fixed distance every x seconds. When it sees someone eaten nearby (`Flee` node) it panics for
  a few seconds (and turns a little yellow): it picks one far point on the other side of the danger (the nearest snake,
  or the spot of the kill), runs there along a NavMesh path without ever turning back, jumping without a pause and
  faster than a hungry snake runs.
- **Snake** (predator): moves at a fixed speed and strolls with rests. When it has not eaten for 8 s (`Hunt` node) it is
  hungry: it turns a little orange, switches to a faster gait (`HuntRun`, set in the `Hunt` config), never stops,
  chases the nearest prey it smells and otherwise wanders to bump into some; if it finds none for 5 s it starves.

## Notes

- Movement is fully physical (forces on the Rigidbody). The NavMesh is only asked for paths.
- The level's four walls are shifted and turned by 90 degrees on every start; `NavMeshNavigationService` then rebuilds the NavMesh around them with a wider agent radius (`NavigationSettings`).
- The fear and hunt radii are drawn as gizmos (turn on Gizmos in the Game view).
- Constructors only assign fields; subscriptions start in `Initialize`/`Start`.
