# Enemy Movement Migration: GameObject → ProjectDawn.Navigation (Hybrid)

Status: **Phases 1-5 all confirmed working in-editor, including the real
`Enemy.cs` (not just the test harness) against real pooling/damage/death.
Phase 6 (roll out to remaining 24 enemy prefabs) in progress — user
requested finishing this and deferring other ideas (broader DOTS adoption,
ability/bullet collision rework) until it's done.** See §7.

**Decisions (2026-07-08/09)**:
- Bridge: use the package's built-in `AgentAuthoring` + `ReadAgentTransformSystem`
  sync (not a hand-rolled `NativeArray<float3>` bridge) as the first cut.
- Avoidance: match Age-of-Sprites — `AgentColliderAuthoring` + `AgentAvoidAuthoring`.
  **Confirmed working in-editor** ("collisions work!").
- Wall fidelity (§4a → superseded by §4b): tier (A) worked but felt
  "clunky" (expected: DOTS position vs. Box2D one-frame-late correction,
  fighting every frame). Went with **Continuum Crowds**
  (`com.projectdawn.navigation.crowds`) instead of the smaller tier (B)
  patch — scoped much smaller than originally estimated once
  `CrowdWorld.SplatObstacleQuad` was found (§4b): no NavMesh baking, no 3D
  geometry conversion needed. **Confirmed working in-editor 2026-07-09**
  after fixing a real bug in the test setup (§4b bug log) — "I can see
  clearly now it's path is respecting the terrain."

Owner: Isaiah
Created: 2026-07-08

## 1. Goal

Replace the current per-`GameObject` `MoveTowards` enemy movement in `2dbullethellgame`
with movement + collision-avoidance math computed in DOTS/ECS (via the
`ProjectDawn.Navigation` package), while keeping enemies as ordinary GameObjects
for everything else (sprite rendering, `Animator`, particles, damage triggers,
object pooling). This is a **hybrid** architecture:

- **DOTS does the math**: agent seeking + separation/avoidance runs as Burst
  jobs over entities, every frame, cheaply, at high enemy counts.
- **GameObjects stay the presentation layer**: `Enemy`/`Slime`/`FlyingEnemy`
  keep their `SpriteRenderer`, `Animator`, `CircleCollider2D` (for the player
  damage trigger), pooling behavior, death/loot logic, etc. — unchanged.
- **The bridge**: each frame, computed entity positions are copied into a
  flat position buffer; a manager component walks the buffer and writes
  `transform.position` on the matching pooled enemy GameObject.

This is explicitly **not** the Age-of-Sprites approach, which renders agents
directly from ECS via NSprites (a custom Entities-Graphics-style renderer —
no GameObjects at all). We only want Age-of-Sprites as a reference for how to
configure `ProjectDawn.Navigation` collision/avoidance in a 2D game; the
GameObject bridge itself is new work specific to this project.

## 2. Current architecture (what we're replacing)

- `Assets/Scripts/Enemies/Enemy.cs` — base MonoBehaviour. `FixedUpdate()` calls
  `Move()`, which does `transform.position = Vector3.MoveTowards(transform.position, player.transform.position, distance)`
  every physics step. No pathfinding, no obstacle avoidance — enemies beeline
  for the player in a straight line. `Slime` overrides `FixedUpdate` similarly;
  `FlyingEnemy` currently adds nothing (its override is commented out).
- Enemies are `Rigidbody2D` (`BodyType 0` = Dynamic, high linear drag) +
  `CircleCollider2D`, but movement bypasses the rigidbody entirely
  (direct `transform.position` writes). The **only** enemy-vs-enemy
  separation happening today is an incidental side effect of Unity's 2D
  physics solver resolving overlapping Dynamic colliders each fixed step,
  fighting against the script's direct position writes. This is expensive
  at scale (full Box2D solve every step for every enemy pair) and not
  actually controlled/tuned — worth confirming in-editor whether it's even
  producing acceptable behavior today, since replacing it is the main
  point of this migration.
- `CircleCollider2D` is also how enemies deal contact damage to the player
  (`OnTriggerStay2D`, checks `Player` layer) — this collider must be kept
  (or reproduced) regardless of how movement is computed.
- Enemies are pooled via `Assets/ObjectPoolManager.cs`
  (`SpawnObject`/`ReturnObjectToPool`, keyed by prefab name, `SetActive`
  toggling, no `Instantiate`/`Destroy` churn during play).
- `gameSupervisorController.cs` spawns enemies in rings around the player,
  `EnemiesPerCooldown` (40 base) growing over time (`GetEnemyCountModifier`)
  — enemy counts are expected to scale into the hundreds over a run. This is
  the actual motivation for the migration: GameObject `MoveTowards` +
  Box2D collision resolution does not scale to bullet-hell enemy counts.
- The game runs entirely on the XY plane (2D physics, `Vector2`/`Vector3`
  with constant-ish Z for sprite sorting). No `Unity.Entities` usage exists
  anywhere in `Assets/` today — this is a from-scratch DOTS integration in
  this project, though the `com.unity.entities` package is already a
  manifest dependency (likely pulled in for the nav package originally).
- No `.asmdef` files exist in `Assets/Scripts` — everything compiles into
  `Assembly-CSharp`. New Burst-compiled ECS code should probably get its own
  `.asmdef` (compile time, and Burst likes assembly boundaries), but it's
  not strictly required since the project doesn't otherwise use them.

## 3. Package status (⚠️ blocking issue found)

`2dbullethellgame/Packages/com.projectdawn.navigation` currently exists but
is **an empty stub** — no `package.json`, no source, just an empty
`Samples~ 1` folder. It is also **not listed** in `Packages/manifest.json`.
Same for `com.projectdawn.navigation.crowds` (only has an empty `Samples~`
tree with the "Board Defense" sample folder, no package.json either).
In short: **the package is not actually installed in this project yet.**

By contrast, `Age-of-Sprites/Packages/com.projectdawn.navigation` is a
complete, working install: v4.0.3, embedded (local, not git/registry),
with `package.json`, full `ProjectDawn.Navigation`, `.Hybrid`, `.Astar`,
`.Editor*`, `ProjectDawn.Entities`, `ProjectDawn.LocalAvoidance`,
`ProjectDawn.ReciprocalAvoidance` source, plus samples (`Scenarios`, `Zerg`,
`Mass`).

**Plan**: copy the full `com.projectdawn.navigation` (and, if we end up
wanting crowd features, `com.projectdawn.navigation.crowds`) package folders
from `Age-of-Sprites/Packages/` into `2dbullethellgame/Packages/`, and add
proper entries to `2dbullethellgame/Packages/manifest.json` as `embedded`
(file-path) dependencies, matching how Age-of-Sprites' `packages-lock.json`
declares them (`"source": "embedded"`).

Version compatibility check (both projects are Unity 2022.3.x, close patch
versions — 2022.3.13f1 here vs 2022.3.17f1 in Age-of-Sprites):

| package | Age-of-Sprites has | 2dbullethellgame has | compatible? |
|---|---|---|---|
| `com.unity.entities` | 1.0.14 | 1.0.16 | yes (nav package only requires ≥1.0.14) |
| `com.unity.ai.navigation` | 1.1.4 | 1.1.5 | yes |
| `com.unity.entities.graphics` | 1.4.5 | 1.0.16 | **not required** — only needed for NSprites-style pure-DOTS rendering, which we are not doing. Do not add/upgrade this for our purposes. |
| `com.unity.physics` | 1.3.5 | not present | not required for our use (no navmesh, no ECS physics needed — see §5) |
| `com.unity.burst` | 1.8.11 | not explicit (pulled transitively) | check after package install |

**Action item**: confirm burst is actually present/pinned once packages are
copied in; add explicit `com.unity.burst` manifest entry if Unity doesn't
resolve one automatically.

## 4. Key package findings (verified by reading package source directly)

These were confirmed by reading the actual package source in
`Age-of-Sprites/Packages/com.projectdawn.navigation/`, not from docs:

- **⚠️ CORRECTION (see §4a below): the "no pathfinding needed" conclusion
  originally here was wrong** — it assumed walls only gated spawn
  placement. There is a real, solid, procedurally-generated wall tilemap
  that currently blocks enemy movement via Box2D physics. Read §4a before
  relying on anything in this bullet.
- `ProjectDawn.Navigation/Locomotion/AgentSeekingSystem.cs`
  shows that with just `Agent` + `AgentBody` + `AgentLocomotion` (no
  `AgentNavMesh`/`AgentAstarPath` component), an agent simply steers straight
  toward `AgentBody.Destination` every frame — i.e. exactly today's
  `MoveTowards` behavior, computed in a Burst job instead. We do **not** need
  `com.unity.ai.navigation` NavMesh baking, no `NavMeshSurface`, no
  `AgentNavMeshAuthoring` — this game has no obstacle-avoidance pathing
  requirement (enemies already beeline for the player today; walls only
  affect where enemies are allowed to *spawn*, via `isPositionInOpenArea`
  in `gameSupervisorController.cs`, not how they move).
- **The package has first-class 2D (XY-plane) support.** `AgentShape.cs`
  defines `ShapeType.Circle` with `Up = (0, 0, 1)` — i.e. the circle shape
  lies flat in the XY plane, normal facing +Z. This is exactly this game's
  convention (2D physics, camera looking down -Z, sprites in the XY plane).
  `ShapeType.Cylinder` (`Up = (0, 1, 0)`) is the 3D/XZ-ground-plane variant
  used for standard 3D navmesh games — **not** what we want. Use
  `AgentCircleShapeAuthoring`, not `AgentCylinderShapeAuthoring`. This means
  **no coordinate remapping is needed** between game space and agent space —
  `float3(x, y, 0)` in ECS lines up directly with the game's `Vector3(x, y, z)`
  (we just never touch the agent's Z / treat it as always 0, and keep sprite
  Z-depth-for-sorting on the GameObject side, outside the synced field).
- **The Hybrid authoring components work fine outside of baking/subscenes.**
  `AgentAuthoring.Awake()` calls `GetOrCreateEntity()` and adds all ECS
  components directly via `EntityManager`, and separately calls
  `manager.AddComponentObject(m_Entity, transform)` to hook into
  `TransformAccessArray`-based sync. This runs from a plain runtime
  `Instantiate()` — it does **not** require the GameObject to live in a
  baked SubScene. This matters because enemies are spawned at runtime via
  `ObjectPoolManager`, not baked. Likewise `AgentAuthoring.OnEnable`/
  `OnDisable` call `EntityManager.SetEnabled(entity, ...)`, which lines up
  1:1 with the pool's `SetActive(true/false)` pattern — enabling/disabling
  the enemy GameObject naturally enables/disables its paired entity's
  simulation for free.
- **The package already ships a built-in GameObject-position bridge**
  (`ProjectDawn.Navigation.Hybrid/Transform/ReadAgentTransformSystem.cs`):
  an `IJobParallelForTransform` that, every frame after the fixed-step
  simulation group, writes each agent entity's `LocalTransform.Position`
  back onto the paired GameObject's `Transform` via `TransformAccessArray`
  — in parallel, Burst-compiled. This is conceptually the same thing our
  brief describes ("store positions, write each enemy GameObject to its
  location"), already built and tested by the package author. **Decided**:
  use this built-in system as-is (via `AgentAuthoring`, one entity per enemy
  GameObject) rather than hand-rolling a flat `NativeArray<float3>` + manual
  copy loop — it already matches the pooling model well and is far less
  code/risk. Revisit a hand-rolled array bridge only if profiling shows the
  `TransformAccessArray` sync is an actual bottleneck at target enemy counts.
  **Strong supporting evidence**: the package's own `Mass` sample
  (`Samples~/Mass/Runtime/Spawner.cs`) does exactly this — runtime
  `GameObject.Instantiate(Prefab, ...)`, `unit.GetComponent<AgentAuthoring>()`,
  then `SetDestination`/`SetDestinationDeferred` — scaling to `MaxCount` up to
  1000 agents, as an official stress test. That's the same scale we need for
  late-game enemy counts, spawned the same way (runtime `Instantiate`, not
  baked). This is close to a direct existence proof that the plain
  `AgentAuthoring` route is fine at our target scale. Prefer
  `SetDestinationDeferred` over `SetDestination` for the per-frame
  "chase the player" call — the sample uses deferred by default, and
  `AgentAuthoring.SetDestination`'s doc comment warns it's a "potentially
  heavy operation" that waits on job completion, whereas deferred just
  queues through an `EntityCommandBuffer`-style singleton.
- **Collision/avoidance is a separate opt-in component, not automatic — and
  "collision" and "avoidance" are two distinct, independently-opt-in
  components, confirmed by reading `AgentColliderAuthoring.cs` and by the
  Age-of-Sprites research below.** `AgentColliderAuthoring` adds an
  `AgentCollider` component: hard circle-circle **non-penetration** (agents
  physically pushed apart on overlap, like a Box2D solver) — this is the
  closer analogue to what our current `Rigidbody2D`+`CircleCollider2D` setup
  is *trying* to do today. Separately, `AgentSeparationAuthoring`
  (`AgentSeparation`: `Radius`, `Weight` 0–1) is a **soft steering force**
  that nudges nearby agents apart preemptively but doesn't guarantee
  non-overlap; `AgentReciprocalAvoidAuthoring` (ORCA-style) and
  `AgentAvoidAuthoring` (cone-based steering avoidance, confirmed used
  in Age-of-Sprites, see below) are two further "soft" alternatives with
  different smoothness/cost tradeoffs. **Gotcha**: the file is named
  `AgentSonarAvoidAuthoring.cs` (menu item "Agents Navigation/Agent Sonar
  Avoid") but the class declared inside is `AgentAvoidAuthoring` — this
  doc originally had the filename wrong as the class name everywhere
  (fixed 2026-07-08); use `AgentAvoidAuthoring` in code, not
  `AgentSonarAvoidAuthoring`. These are meant to be layered:
  hard `AgentCollider` for guaranteed non-overlap, plus one soft avoidance
  component on top for less jittery approach behavior.

## 4a. Terrain is a real physical obstacle, not just a spawn-gate (correction)

The user flagged this mid-session: **the level has procedurally-generated
wall terrain that needs to be accounted for.** Verified directly:

- `Assets/Scripts/Random Map Generation/MapGenerator.cs` (`LevelGenerator`)
  procedurally generates a tilemap using noise (Perlin / cellular automata /
  directional tunnel, selectable per `MapSettings` asset). There are two
  separate `MapSettings` assets in the project, `Walls.asset` and
  `Floors.asset` — these drive **two separate `LevelGenerator` tilemaps**,
  confirmed by `Walls.asset` using a cave/tunnel-shaped algorithm
  (`CellularAutomataMoore`-family settings: `fillAmount`, `smoothAmount`,
  `edgesAreWalls: 1`), i.e. a real obstacle layout, not just decoration.
- Confirmed in `Assets/Scenes/Game.unity`: there is a GameObject named
  `Walls` on **physics layer 3 ("Walls"**, per `ProjectSettings/TagManager.asset`)
  carrying a real `TilemapCollider2D`.
- Confirmed in `ProjectSettings/Physics2DSettings.asset`: the layer
  collision matrix is the Unity default (all `f`s — every layer collides
  with every other layer). Nothing has been specially excluded, so today
  Enemy-vs-Enemy **and** Enemy-vs-Wall Box2D collision resolution are both
  active simultaneously.
- Net effect on **current** behavior: since `Enemy.Move()` sets
  `transform.position` directly every `FixedUpdate` (straight line toward
  the player, oblivious to walls), and enemies are `Dynamic` `Rigidbody2D`s,
  Unity's Box2D solver detects the resulting overlap against the static
  `Walls` `TilemapCollider2D` and corrects it out on the *next* step — but
  then `Move()` immediately re-teleports toward the player again next
  frame, undoing the correction. The likely visible result is enemies that
  press/jitter against a wall and can slide along it (any movement
  component parallel to the wall surface is unaffected) but can't cleanly
  path around it — **not real pathfinding, but not a total no-op either.**
  This needs an in-editor sanity check (spawn some enemies behind a wall
  segment from the player and watch what actually happens today) before
  either assuming it's fine to reproduce or deciding it needs fixing —
  tracked as a Phase 0 verification item below.

**This invalidates the earlier "no pathfinding needed" framing** in
§4/§6 as originally written — it was reasoned from the wrong assumption
that walls only gated spawn position. The real question is how much wall
fidelity to preserve when movement authority moves to DOTS, since DOTS
agent seeking (§4) has **zero knowledge of Box2D colliders** — an
`AgentLocomotion`-driven entity will seek straight through a wall unless
we do something about it. Three options, increasing in cost/fidelity:

- **(A) Minimal — reproduce today's behavior as-is.** Keep using the
  package's built-in `ReadAgentTransformSystem` (writes DOTS position
  straight to `Transform.position`, §4). Enemy stays a `Dynamic`
  `Rigidbody2D` on the `Enemy`/`FlyingEnemy` layer, `Walls` layer collision
  stays enabled. Box2D will fight the DOTS-driven position exactly like it
  fights `Move()` today (one-frame-late correction, same jitter/slide
  characteristics as current behavior). Enemy-vs-Enemy Box2D collision can
  (and should) be **disabled** in the layer collision matrix now that DOTS
  owns agent-vs-agent separation/avoidance (§4's `AgentColliderAuthoring`/
  `AgentAvoidAuthoring`) — that removes the expensive O(n²)-ish
  dynamic-dynamic solve that was the actual performance motivation for this
  migration, while leaving the comparatively cheap, sparse Enemy-vs-Wall
  solve untouched. No NavMesh, no new wall representation in ECS at all.
- **(B) Physics-correct, still no real pathfinding.** Same as (A), but
  replace the package's default `TransformAccess`-based write with a small
  custom bridge job that calls `Rigidbody2D.MovePosition(desiredPos)`
  instead of setting `Transform.position` directly — `MovePosition` is
  physics-aware and gets resolved against the `Walls` collider in the same
  step instead of one-frame-late, so enemies would cleanly stop/slide at a
  wall instead of visibly jittering into it. Enemies still won't route
  *around* a wall between them and the player — they can get stuck exactly
  at the boundary. Meaningfully more custom code than (A) (this is exactly
  the "hand-roll the bridge" fork from §4/§6 — note that choosing (B) means
  we don't get the built-in `ReadAgentTransformSystem` for free after all,
  since it doesn't know about `Rigidbody2D`).
- **(C) Full pathfinding around walls.** Runtime-bake a Unity `NavMesh`
  (`com.unity.ai.navigation`, `NavMeshSurface.BuildNavMesh()`, which does
  support runtime rebaking) from the `Walls` tilemap after every
  `GenerateMap()` call, and use `AgentNavMeshAuthoring` per enemy for real
  obstacle routing. Significantly more scope: Unity's NavMesh baking is
  inherently 3D/XZ-plane regardless of our XY `AgentCircleShapeAuthoring`
  choice for local avoidance/collision (§4), so this needs a tilemap→3D
  geometry conversion (e.g. extrude wall cells into thin XZ-plane boxes)
  and a coordinate-conversion layer between navmesh-space (XZ) and our
  gameplay space (XY) — plus a rebake pass wired into `LevelGenerator.GenerateMap()`
  every time the map regenerates. This is the only option that gives actual
  "route around the wall to reach the player" behavior; (A) and (B) both
  just stop cleanly (or jitter) at the wall boundary, same ceiling as today.

**Recommendation**: start with **(A)** — it's a strict subset of work
already planned (§6), reproduces current behavior, and removes the actual
performance-motivating cost (enemy-vs-enemy Box2D solving at scale) without
touching wall handling at all. Upgrade to (B) only if in-editor testing
shows (A)'s jitter is worse than today's baseline (it shouldn't be — same
mechanism). Treat (C) as a separate, later feature request, not part of
this migration, unless testing reveals current wall behavior is already
unacceptably bad and worth fixing at the same time.

**Superseded 2026-07-08**: user tested tier (A) in-editor — "the current
collider solution does work but it feel clunky" — and asked whether the
tilemap collider itself could be made "agent friendly." Answer: no, not by
tweaking `TilemapCollider2D` settings — the clunkiness is architectural
(DOTS computes a position every frame with zero knowledge of the wall
collider; Box2D can only correct it one frame late; they fight forever).
Went ahead and scoped tier (C) properly instead of leaving it deferred —
see §4b. User chose **Continuum Crowds** over tier (B).

## 4b. Wall routing via Continuum Crowds (chosen, in progress)

Investigated whether the base package's `AgentColliderAuthoring` (the "hard
collision" already confirmed working between enemies, §5) could be
repurposed to represent static wall obstacles. **No** — confirmed by
reading `AgentColliderJob` directly: both sides of any collision resolution
must carry `AgentBody`, and `AgentMotionType.Static` entities explicitly
never get one (`"Agent is static. It's entity will not have AgentBody..."`).
The base package's collision system is fundamentally agent-vs-agent; there
is no static-obstacle concept in it at all.

The `com.projectdawn.navigation.crowds` add-on (previously deferred, §5/§6)
has the right tool for this: `CrowdSurfaceAuthoring` + `CrowdWorld`, a
plain 2D grid of obstacle cells that agents flow around via a computed flow
field (`AgentCrowdPathingAuthoring`) — independent of Unity NavMesh, no 3D
geometry or raycasting involved. Original tier (C) scoping (§4a) assumed
this would still require a tilemap→3D-geometry conversion; that assumption
was wrong. Confirmed by reading the source directly:

- `CrowdSurfaceAuthoring` just needs `Width`/`Height` (cell counts) and
  `Size` (world-space size) — a flat 2D grid definition, no baked NavMesh
  underneath it at all.
- `CrowdWorld.SplatObstacleQuad(worldPosition, size, opacity)` (in
  `ProjectDawn.ContinuumCrowds/CrowdWorld.cs`) stamps an obstacle into the
  **live**, already-created grid at a given world position — no
  `CrowdData` ScriptableObject pre-baking needed at all. This is a public,
  documented part of the API (`SplatObstacleQuad`/`SplatObstacleCircle`),
  not an internal we're reaching around.
- This means we can loop over exactly the same wall layout
  `LevelGenerator` already rendered (read back via `Tilemap.HasTile`, not
  duplicating its generation logic) and stamp each wall cell in directly —
  no NavMesh baking, no raycasting, no 3D conversion.
- **Timing hazard found and worked around**: `CrowdSurfaceSystem.OnUpdate()`
  only reads a `CrowdData` asset's baked arrays **once**, on the frame the
  surface entity is first seen (`Entities.WithNone<CrowdSurfaceWorld>()`).
  Pre-baking a `CrowdData` asset from `LevelGenerator.GenerateMap()` would
  race this — whichever happens first each Play session wins, and Unity's
  exact ordering between this system's `InitializationSystemGroup` tick and
  our own `MonoBehaviour.Start()` calls isn't something we can verify
  without testing in the Editor. Splatting into the **already-live**
  `CrowdWorld` instead sidesteps this entirely — there's no ordering
  requirement, it can happen any time after both the surface entity and
  the tilemap exist, which is exactly what `CrowdWallBaker` does (waits on
  `HasComponent<CrowdSurfaceWorld>` in a coroutine, then stamps).
- Grid geometry confirmed directly from `Game.unity` / the `Grid` component
  (not assumed): **100×100 cells, 0.24 unit cell size** (`m_CellSize: {x:
  0.24, y: 0.24, z: 0}`), origin at world `(0,0,0)` — giving a ~24×24 unit
  arena, consistent with `gameSupervisorController.isPositionInSpawnArea`'s
  existing `0..23` bounds check. `CrowdSurfaceAuthoring`'s `Width=100,
  Height=100, Size=(24,24)`, positioned at world `(0,0,0)`, lines up 1:1
  with our tile grid — confirmed by reading `CrowdData.BuildHeightFieldFromColliders`'s
  own local→world conversion (`(x+0.5, y+0.5)` in cell space transformed by
  the surface's `NonUniformTransform`), which matches Unity's own
  `Tilemap.GetCellCenterWorld` convention when the surface origin matches
  the tilemap/grid origin.
- `AgentCrowdPathingAuthoring` (per-enemy) is purely additive —
  `[RequireComponent(typeof(AgentAuthoring))]`, doesn't replace or
  conflict with `AgentColliderAuthoring`/`AgentAvoidAuthoring`. Its default
  `GoalSource` is `CrowdGoalSource.AgentDestination`, meaning it reads the
  same `AgentBody.Destination` we already set via
  `SetDestinationDeferred(player.position)` — no change needed to how
  destinations get set.
- **Real bug caught before it shipped**: `AgentCrowdPathingAuthoring.m_Group`
  needs to reference the scene's single `CrowdGroupAuthoring` (all enemies
  share one group/goal, since they all chase the same player). Originally
  wired this via `SerializedObject` in the same Editor tool that edits
  **prefab assets** — but Unity silently strips direct scene-object
  references from prefab assets on save, so every enemy prefab would have
  ended up with a dangling null `Group`. Fixed by adding
  `GlobalReferences.crowdGroup` (mirrors the existing `GlobalReferences.player`
  pattern already used throughout the codebase) — a new `CrowdGroupRegistrar`
  component publishes the scene's group to it in `Awake()`, and
  `NavTestSeeker.Start()` (eventually `Enemy.cs`, Phase 4) reads it from
  there instead. Prefab assets now only get the bare `AgentCrowdPathingAuthoring`
  component added (structurally fine to bake in), never a scene reference.

**Implementation (2026-07-08)**:
- Installed `com.projectdawn.navigation.crowds` v1.0.1 (same copy-from-Age-of-Sprites
  pattern as §3, 305/305 files verified).
- `Assets/Scripts/Navigation/CrowdWallBaker.cs` (new, runtime): on the Walls
  `LevelGenerator`'s `OnGenerated` event, waits for `CrowdSurfaceWorld` to
  exist, then stamps every currently-rendered wall tile into the live
  `CrowdWorld` via `SplatObstacleQuad`.
- `Assets/Scripts/Random Map Generation/MapGenerator.cs`: added
  `public UnityEvent OnGenerated`, invoked at the end of `GenerateMap()`.
  Minimal, additive change (mirrors the existing `Enemy.OnEnemyDeath`
  `UnityEvent` pattern already used in this codebase). Confirmed both the
  Walls and Floors `LevelGenerator` instances reliably fire this — each
  calls `GenerateMap()` on **itself** unconditionally from its own
  `Start()` (`LevelGenerator.cs:37`), independent of the
  `GlobalReferences.levelGenerator` singleton ambiguity that exists between
  the two instances (that ambiguity is pre-existing, not introduced here,
  and doesn't affect this since we listen on the specific Walls instance
  directly, not through the singleton).
- `Assets/Scripts/Navigation/CrowdGroupRegistrar.cs` (new, runtime):
  publishes its `CrowdGroupAuthoring` to `GlobalReferences.crowdGroup` in
  `Awake()`.
- `Assets/Scripts/GlobalReferences.cs`: added `public static CrowdGroupAuthoring crowdGroup`.
- `Assets/Scripts/Navigation/NavTestSeeker.cs`: `Start()` now also wires
  `AgentCrowdPathingAuthoring.Group = GlobalReferences.crowdGroup` if the
  component is present (safe to call after `OnEnable`, per the setter's
  own null-entity guard).
- `Assets/Editor/Navigation/AgentAuthoringSetupTool.cs`: new menu command
  **`Tools/Navigation/Setup Crowd Surface In Open Scene`** — finds the
  Walls `LevelGenerator` (by `mapSetting.name == "Walls"`, not a hardcoded
  GUID), creates `Crowd Surface (Walls)` (`CrowdSurfaceAuthoring`, sized
  from the generator's actual `width`/`height`/`tilemap.cellSize`),
  `Crowd Group (Enemies)` (`CrowdGroupAuthoring` + `CrowdGroupRegistrar`),
  and `Crowd Wall Baker` (`CrowdWallBaker`, wired to the Walls generator/
  tilemap/surface) — all via Unity's own `AddComponent`/`Undo` APIs, same
  safe scene-mutation pattern as the rest of this tooling (§ Phase 1d).
  `ApplyAgentSetup` now also adds bare `AgentCrowdPathingAuthoring` to every
  enemy it touches (no group reference set there — see bug fix above).

**Known unknowns going into testing** (flagging clearly — this is the
least-tested part of the whole migration so far, and involves package
internals (`CrowdWorld`, `ICleanupComponentData`) I could only verify by
reading source, not by running it):
- Whether `CrowdSurfaceWorld` actually appears within the ~120-frame guard
  `CrowdWallBaker` waits (should be near-instant, but unverified).
  If it times out, this is the first place to look.
- Whether the grid alignment assumption (`CrowdSurfaceAuthoring` at world
  `(0,0,0)` matching the tilemap/grid origin) is actually correct — if
  enemies avoid the wrong cells (offset from the real walls), this is the
  most likely cause.
- Whether `SplatObstacleQuad`'s `size` parameter wants full cell size or
  half-extents (the doc comment doesn't specify) — if walls seem too
  thick/thin or have gaps, try halving/doubling the passed size.

**Resolved — testing log (2026-07-08/09):**
- `CrowdWallBaker` logged `Stamped 3002 wall cell(s)...` immediately — no
  timeout, `CrowdSurfaceWorld` timing was a non-issue in practice.
- First confusing result: with the enemy's own `CircleCollider2D` enabled,
  it appeared to avoid walls; disabling it let it walk through terrain.
  This turned out to be a red herring — that collider is Box2D colliding
  with the Walls `TilemapCollider2D` (tier A behavior, still physically
  present and untouched by any of this work), not evidence Crowds was
  doing anything. Box2D would produce that exact same appearance whether
  or not Crowds was contributing at all, so this test didn't actually
  isolate anything.
- Used the package's built-in debug gizmos (`CrowdGroupAuthoring`'s
  Inspector, "Gizmos" dropdown → Potential/Velocity, visible in the Scene
  view during Play) to actually look at the flow field: **Potential mode
  showed a uniform white field with no gradient, and Velocity mode showed
  no arrows at all.** Traced this to what that pattern means:
  `RecalculatePotentialField` seeds its search only from `GoalCells`; with
  zero goals the search never starts, the field stays at its "unreached"
  sentinel value (`10000`) everywhere, and that clamps to uniform white
  when rendered — exactly the observed symptom.
- Root cause: **the specific test enemy instance predated the crowd
  surface being set up.** `AgentCrowdPathingAuthoring.Group` gets wired at
  runtime from `GlobalReferences.crowdGroup` (§4b bug fix, prefab assets
  can't hold a direct scene reference) — but that only happens once, in
  `NavTestSeeker.Start()`. An enemy spawned *before* `Setup Crowd Surface
  In Open Scene` had already run `Start()` with `GlobalReferences.crowdGroup`
  still null, so its `Group` stayed `Entity.Null` forever — `CrowdGoalSystem`
  silently skips adding goals for entities in a null group, explaining the
  empty flow field precisely. Not a bug in the wiring code itself, just a
  stale test object — deleting it and re-spawning via the same menu command
  (now with the crowd group already present in the scene) fixed it
  immediately: **"after respawning the enemy, and moving it far away, I
  can see clearly now it's path is respecting the terrain."** Confirmed
  working end-to-end.
- Practical implication for Phase 4/6: whatever eventually assigns
  `AgentCrowdPathingAuthoring.Group` on real enemies (mirroring
  `NavTestSeeker.Start()`) needs to run any time `GlobalReferences.crowdGroup`
  might not have been set yet when that enemy's own `Start()`/`OnEnable`
  ran — pooled enemies reused across a run should be fine (one-time app
  startup ordering), but this is worth an explicit check during Phase 5
  (pool lifecycle) rather than assuming it's automatically fine.

## 5. Age-of-Sprites deep-dive findings (confirmed)

A full research pass into `Age-of-Sprites`' actual enemy/unit setup is done.
Key results (all confirmed directly from prefab YAML and package source, not
docs):

- **Only 4 prefabs in the whole project use ProjectDawn agent authoring**:
  `Slime.prefab`, `Slime_MeshRendered.prefab`, `Pawn Blue.prefab`, and
  `Player.prefab`. Each carries `AgentAuthoring` + `AgentColliderAuthoring` +
  `AgentCircleShapeAuthoring` — **confirming the XY-plane prediction**:
  `AgentCylinderShapeAuthoring` is never used anywhere in the project.
  Verified directly in `AgentColliderSystem.cs`: the collider system has two
  separate code paths keyed off `ShapeType` —
  `ShapeType.Cylinder` collides using `.xz` (3D/Y-up ground plane) while
  `ShapeType.Circle` collides using `.xy` (our plane), e.g.
  `float2 towards = Transform.Position.xy - otherTransform.Position.xy; ... action.Displacement = new float3(action.Displacement.xy, 0);`.
  This is a real, intentional feature of the package, not an accident — it
  directly validates §4's "no coordinate remapping needed" conclusion.
- **Slime's actual production settings**
  (`AgentAuthoring`: `MotionType=DefaultLocomotion, Speed=0.75, Acceleration=4, AngularSpeed=120, StoppingDistance=0, AutoBreaking=0`;
  `AgentColliderAuthoring`: `Layers=1`;
  `AgentCircleShapeAuthoring`: `Radius=0.2`;
  `AgentAvoidAuthoring`: `Radius=0.3, Angle=230, MaxAngle=300, Mode=3, BlockedStop=1, UseWalls=0`)
  — **`AgentSeparationAuthoring` and `AgentReciprocalAvoidAuthoring` are
  never used anywhere in the project.** Production collision/avoidance for
  Slime = `AgentColliderAuthoring` (hard push-apart) + `AgentAvoidAuthoring`
  (cone-based steering avoidance), not Separation. This directly overrides
  §4's earlier speculative recommendation of plain `AgentSeparation` — we
  should default to the Collider + Sonar combo Age-of-Sprites actually ships
  with, matching the ask to use it "as an example."
- **Slime also uses Continuum Crowds flow-field pathing**
  (`AgentCrowdPathingAuthoring`, from the separate `com.projectdawn.navigation.crowds`
  add-on) baked against a `CrowdSurfaceAuthoring` in a SubScene — i.e.
  Age-of-Sprites' own reference solution for "agents that must navigate
  around obstacles" is this flow-field crowds add-on, not plain NavMesh
  (`AgentNavMeshAuthoring` is unused there too, despite a `NavMeshSurface`
  sitting unused in the same scene — see §4a). **Revised conclusion after
  §4a**: we now know our walls are real obstacles, not just a spawn-gate,
  so this is no longer an easy "skip it" call in principle — flow fields
  are actually a good conceptual fit for a noise-generated tile grid (a
  flow field is itself grid-based, recomputable per region). That said, per
  §4a's tiered recommendation, we're deferring real obstacle routing to
  tier (C) and starting with tier (A) (no pathing, matches current
  behavior), so the crowds add-on stays out of scope **for now** — but if
  tier (C) is ever pursued, `com.projectdawn.navigation.crowds` +
  `AgentCrowdPathingAuthoring` (regenerating the crowd surface after each
  `GenerateMap()`) is the Age-of-Sprites-proven pattern to copy, likely a
  better fit than plain NavMesh baking for a tile-grid-based level.
- **Notable pattern**: `Player.prefab` also carries `AgentAuthoring`
  (`MotionType=Dynamic`, no locomotion) + `AgentCircleShapeAuthoring` +
  `AgentColliderAuthoring`, purely so the player participates in
  spatial-partitioning/collision against enemies — its actual position is
  driven by a hand-rolled system (`PlayerSystem.cs`: `position += input * speed * dt`),
  not by agent locomotion. This is a reusable trick if we ever want the
  bullet-hell player to physically push through/against enemies via the same
  collision system, but is **not required** for this migration (the player
  stays pure GameObject/Rigidbody2D — only enemies move to DOTS).
- **Everything is baked-subscene + pure-ECS spawning** (`Baker<T>` on each
  prefab, then `FactorySystem`/`ecb.Instantiate(bakedEntityPrefab)` at
  runtime — no `GameObject.Instantiate` for enemies at all), which is why
  Age-of-Sprites has no need for a DOTS→GameObject bridge itself (rendering
  is pure-ECS via NSprites, confirmed no `SpriteRenderer`/`MeshRenderer`
  GameObjects exist at runtime for units). **We cannot copy this spawning
  pattern** — we need runtime `GameObject.Instantiate`-compatible agents
  (via plain `AgentAuthoring.Awake()`, confirmed working outside baking in
  §4, and validated at scale by the package's own `Mass` sample). The
  bridge itself (DOTS position → GameObject transform) has no Age-of-Sprites
  precedent to copy; the closest template remains the package's own
  `ReadAgentTransformSystem.cs` (§4).

## 6. Target architecture (draft)

1. **Package install**: copy `com.projectdawn.navigation` from Age-of-Sprites
   into this project, wire up `manifest.json` (§3). Do **not** copy
   `com.projectdawn.navigation.crowds` yet — not needed for tier (A)/(B)
   (§4a); revisit if tier (C) obstacle-routing is ever pursued (§5).
2. **Per-enemy authoring**: add `AgentAuthoring` (`DefaultLocomotion` motion
   type) + `AgentCircleShapeAuthoring` (radius ≈ current `CircleCollider2D`
   radius) + `AgentColliderAuthoring` + `AgentAvoidAuthoring` to each
   enemy prefab, mirroring Age-of-Sprites' Slime settings as a tuning
   starting point (§5), instead of `AgentSeparationAuthoring` as originally
   speculated. Confirm Sonar's cone-based avoidance still looks/feels right
   for a bullet-hell mob converging on one point (vs. Slime's RTS-unit
   context) during Phase 3 prototyping — it's an easy swap to plain
   `AgentColliderAuthoring`-only, or to `AgentSeparationAuthoring`, if not.
   Given ~15+ enemy prefabs, add these at runtime via a bootstrap
   MonoBehaviour or `RequireComponent` chain rather than hand-editing every
   prefab in the inspector (or investigate a `PrefabUtility` batch script).
3. **Movement authority moves out of `Enemy.cs`**: `Move()`/`SlimeMove()`
   stop calling `Vector3.MoveTowards`. Instead, each enemy keeps its
   `AgentAuthoring` reference and calls `SetDestination(player.position)`
   (or `SetDestinationDeferred`) once per frame (or only when the player
   moves past some threshold, to avoid redundant destination churn — TBD,
   watch `AgentBody.SetDestination` cost). Actual position writes happen
   via the package's `ReadAgentTransformSystem` (§4) — `Enemy.cs` no longer
   writes `transform.position` for movement at all.
4. **Keep everything else in `Enemy.cs` as-is**: health, damage,
   `OnTriggerStay2D` contact damage, death/pooling, sprite flip based on
   player position, animator speed — none of this needs to change, it just
   stops being the thing that also happens to move the transform.
5. **Pool lifecycle**: verify `AgentAuthoring`'s `OnEnable`/`OnDisable`
   (`SetEnabled`) correctly pauses/resumes simulation across
   `ObjectPoolManager` activate/deactivate cycles, and that re-`SetDestination`
   on reactivation doesn't leave stale state (old destination, old velocity)
   from the previous life of the pooled object. Likely needs an explicit
   `Stop()` + fresh `SetDestination()` call in `Enemy.OnEnable()`.
6. **Rigidbody2D/CircleCollider2D (revised per §4a tier A)**: keep the
   enemy's `Rigidbody2D` as **Dynamic** and keep `CircleCollider2D` enabled
   — do **not** make it Kinematic, and do not remove the rigidbody. We
   *want* Box2D to keep resisting wall penetration exactly like it does
   today (§4a tier A). What changes: **disable Enemy-vs-Enemy (and
   Enemy-vs-FlyingEnemy) collision** in `ProjectSettings/Physics2DSettings.asset`'s
   layer collision matrix, now that DOTS (`AgentColliderAuthoring` +
   `AgentAvoidAuthoring`) owns agent-vs-agent separation — that's the
   expensive O(n²)-ish part physics was doing that we actually want to
   remove. Enemy-vs-Wall and Enemy-vs-Player (trigger) collision stay
   enabled and unchanged. This also sidesteps needing to represent walls in
   ECS at all for tier (A)/(B).
7. **Bridge granularity decision**: default to package's built-in
   `TransformAccessArray` sync (`ReadAgentTransformSystem`, writes straight
   to `Transform.position`) for tier (A) — confirmed compatible with
   keeping the `Rigidbody2D` Dynamic (§4a: this reproduces today's
   "script sets position, Box2D corrects next frame" wall interaction, just
   now for DOTS-computed positions instead of `MoveTowards` output). Only
   move to a custom `Rigidbody2D.MovePosition`-based bridge if tier (B) is
   later chosen (§4a) — that decision is independent of the
   hand-rolled-`NativeArray` question, which stays deferred to a
   profiling-driven follow-up either way.

## 7. Implementation phases (checklist, will expand)

- [ ] Phase 0 — Land this document, confirm open questions with user.
      Includes: in-editor sanity check of **current** enemy-vs-wall behavior
      (spawn enemies behind a `Walls` tilemap segment from the player, watch
      whether they jitter/stick/slide) to have a real baseline before
      judging whether tier (A)'s "reproduce current behavior" is actually
      acceptable (§4a).
- [x] Phase 1a — Copy `com.projectdawn.navigation` v4.0.3 from Age-of-Sprites
      into `2dbullethellgame/Packages/` (2026-07-08). Replaced the previous
      empty stub (which git confirms was never tracked — not gitignored,
      not in history, so nothing was lost). 620/620 files verified copied.
      `manifest.json` deliberately left untouched, mirroring Age-of-Sprites'
      own working setup — Unity auto-discovers embedded packages from a
      `package.json` in `Packages/` without needing a manifest entry;
      `packages-lock.json` will populate on next Editor domain reload.
- [ ] Phase 1b — **Handed to user (2026-07-08)**: they'll verify in their
      own Editor rather than have this run through a version-mismatched
      local install (only 2022.3.41f1/6000.0.32f1 installed, project pins
      2022.3.13f1). Combine with Phase 0 and Phase 2 below into one
      in-editor session — see "Next steps for your Editor session" at the
      end of this section.
- [x] Phase 1c — Built `Assets/Editor/Navigation/AgentAuthoringSetupTool.cs`
      (2026-07-08): a `Tools/Navigation/Add Agent Navigation To Selected
      Prefabs` menu command. Select one or more enemy prefabs in the
      Project window, run it, and it adds (idempotently — safe to re-run,
      only fills in missing components) `AgentAuthoring` +
      `AgentCircleShapeAuthoring` (radius copied from the prefab's existing
      `CircleCollider2D`) + `AgentColliderAuthoring` + `AgentAvoidAuthoring`,
      tuned per §6/§5 (Age-of-Sprites' Slime settings, radius-scaled to each
      prefab's own collider). Notably sets `StoppingDistance=0`/
      `AutoBreaking=false` on `AgentAuthoring` — the package defaults
      (`0.1`/`true`) would make enemies brake and stop just short of the
      player, breaking the contact-damage trigger, which only fires because
      the player carries a **trigger** collider that overlaps the enemy's
      **solid** (non-trigger, confirmed `m_IsTrigger: 0` on e.g. `Green
      Cube.prefab`) `CircleCollider2D` — enemies must fully close distance
      and stay pressed against the player, not stop short like an RTS unit
      reaching a waypoint. This tool is reused for both Phase 2 (prototype)
      and Phase 6 (full rollout) so there's exactly one place tuning values
      live, instead of duplicating them.
- [x] Phase 1d — Built the rest of the Phase 2 smoke-test harness
      (2026-07-08), directly in the real `Game` scene rather than a separate
      test scene, since hand-editing `Game.unity`'s YAML directly (131k
      lines, 35 GameObjects, dense cross-references) was judged too risky
      to do blind without Play Mode access to validate it — safer to build
      one more Editor-menu tool that mutates the scene through Unity's own
      APIs when *you* click it (undo-safe, visible before you ever save):
      - `Assets/Scripts/Navigation/NavTestSeeker.cs` — tiny runtime
        `MonoBehaviour`, calls `AgentAuthoring.SetDestinationDeferred(GlobalReferences.player.transform.position)`
        every frame. Temporary — delete once Phase 4 lands the real
        `Enemy.cs` integration.
      - New menu command **`Tools/Navigation/Spawn Nav Test Enemy In Open
        Scene`** (same file as the Phase 1c tool): instantiates a
        `Green Cube.prefab` instance directly into whatever scene you have
        open, disables its `Enemy` component (so the old
        `Vector3.MoveTowards` in `FixedUpdate` doesn't fight the new
        DOTS-driven position — Phase 2 is testing the movement bridge in
        isolation), runs the same `ApplyAgentSetup` the prefab tool uses,
        adds `NavTestSeeker`, and pings/selects it in the Hierarchy. Fully
        undoable (`Ctrl+Z` removes it), or just delete the GameObject when
        done — nothing is written to disk unless you save the scene.
      - `Walls.asset`: temporarily flipped `randomSeed` from `1` (true) to
        `0` (false) for a reproducible layout across Play sessions while
        debugging (§4b). **Reverted back to `1` on 2026-07-09** now that
        wall-routing is confirmed working end-to-end — each Play session
        (confirmed there's no mid-session "round" system; a fresh tilemap
        is generated once, every time you press Play, via
        `LevelGenerator.Start()`) goes back to a genuinely random layout via
        `Time.time`. No code change was needed to make `CrowdWallBaker`
        keep working with this — it already re-bakes on every
        `LevelGenerator.OnGenerated` firing, regardless of whether the seed
        behind that generation is fixed or random.
- [x] Phase 2 — **Confirmed working (2026-07-08)**: spawned via **Tools ▸
      Navigation ▸ Spawn Nav Test Enemy In Open Scene**, moved toward the
      player, Burst compiled cleanly. Package install (Phase 1a/1b) and the
      movement bridge (§4) are both validated end-to-end. Two issues found
      and fixed in the tooling itself (not one-off test hacks — both are
      real, permanent settings every enemy needs):
      - **Rotation**: the sprite was rotating to face travel direction.
        Traced to `AgentLocomotionSystem.cs`: it always slerps
        `LocalTransform.Rotation` toward the facing angle using
        `AngularSpeed` as the interpolation rate — but `slerp(a, b, 0) == a`,
        so setting `AngularSpeed=0` permanently disables rotation with zero
        effect on movement math. Added to `ApplyAgentSetup` in the Phase 1c
        tool, so every prefab it touches (including ones already set up —
        just re-run the tool) gets this. Our sprites stay upright and use
        the existing horizontal-flip logic in `Enemy.FixedUpdate`, not
        transform rotation.
      - **Speed**: felt "very fast" — root cause confirmed as the package
        default `Speed=3.5`, while Green Cube's actual `Attributes` asset
        has `moveSpeed=0.15` (23x slower). `NavTestSeeker` didn't wire
        `Attributes` in at all (it's a bare movement-bridge smoke test).
        Fixed by giving `NavTestSeeker` a `TestSpeed` field (default `2`,
        set via `AgentAuthoring.EntityLocomotion` in `Start()`) as a
        reasonable placeholder — real per-enemy speed will come from
        `Attributes.GetAttribute(Attribute.moveSpeed)` in Phase 4, same as
        `Enemy.OnEnable()` already does today for the old system.
- [x] Phase 3 — **Confirmed working (2026-07-08)**: "collisions work" per
      user — `AgentColliderAuthoring` + `AgentAvoidAuthoring` (added by the
      Phase 1c tool) are keeping agents from stacking. No swap to
      `AgentSeparationAuthoring` needed unless further testing with a full
      mob surfaces problems with the forward-cone model (§4a decisions).
- [x] Phase 4 — **Implemented 2026-07-09**, not yet tested in-editor.
      `Assets/Scripts/Enemies/Enemy.cs`:
      - `Awake()` caches `GetComponent<AgentAuthoring>()` into a new
        `protected AgentAuthoring _agent` field — **null if the prefab
        hasn't been migrated yet** (Phase 6), so `Move()` and pool
        lifecycle both branch on this and fall back to the original
        `Vector3.MoveTowards` path for un-migrated prefabs. This means
        prefabs can be migrated one at a time (run the Phase 1c tool on
        just one, playtest, repeat) rather than needing a single big-bang
        cutover of all ~15+ prefabs at once.
      - `Move()`: when `_agent != null`, calls
        `_agent.SetDestinationDeferred(player.transform.position)` instead
        of `Vector3.MoveTowards` — the actual position write now happens
        via the package's `ReadAgentTransformSystem` (§4), not this method.
      - `suspendActions` (the player-death freeze feature,
        `PlayerDeathHandler.cs`) needed explicit handling: unlike the old
        system where simply not calling `Move()` left `transform.position`
        untouched (implicitly frozen), `AgentLocomotionSystem` keeps
        driving a DOTS agent toward its last-set destination every frame
        regardless of whether `Move()` ran that frame — so "just don't call
        SetDestination" would NOT actually freeze a migrated enemy, a real
        regression. Added `_agent.Stop()`, called once on the
        suspended-transition (not every frame — its doc comment warns it
        waits for agent jobs to finish, so calling it every frame while
        suspended would be a real perf hazard). Extracted as
        `protected StopAgentOnceIfSuspended()` since `Slime.cs` checks
        `suspendActions` and returns *before* ever calling `Move()`
        (`Slime.FixedUpdate()` updated to call the new helper directly in
        that branch) — `FlyingEnemy.cs` needed no change since its override
        is fully commented out, inheriting base `Enemy.FixedUpdate()`
        unchanged.
      - `OnEnable()` (pool reactivation): three things needed handling that
        `NavTestSeeker`'s simpler harness didn't have to deal with —
        (1) **position resync**: `ObjectPoolManager.SpawnObject` sets
        `transform.position` to the new spawn point before
        `SetActive(true)`, but `AgentAuthoring` only ever captures
        `transform.position` into the entity once, in its own `Awake()` —
        without manually resyncing `LocalTransform.Position` here, a
        reused pooled enemy would keep its entity's position from wherever
        it last died and visibly slide across the map to the new spawn
        point instead of appearing there. (2) `_agent.Stop()` to clear
        stale velocity/destination from the enemy's previous life
        (one-time cost per activation, not a per-frame hot path, so the
        "waits for agent jobs" cost is acceptable here unlike in `Move()`).
        (3) Sets `EntityLocomotion.Speed = speed * speed_animation_multiplier`
        once here (real per-enemy value from `Attributes.moveSpeed`,
        replacing `NavTestSeeker.TestSpeed`'s placeholder) — deliberately
        *not* set every frame in `Move()`, since `EntityLocomotion`'s
        getter/setter is also documented as a job-sync point;
        `speed_animation_multiplier` is confirmed unused/always-1 elsewhere
        in the codebase, so a per-frame update isn't currently needed
        anyway. Also wires `AgentCrowdPathingAuthoring.Group` from
        `GlobalReferences.crowdGroup` here, mirroring
        `NavTestSeeker.Start()` (§4b bug log) — same reasoning: prefab
        assets can't hold a direct scene-object reference.
      - **Confirmed working against the real pooling/death/damage flow
        2026-07-09** on `Green Cube` — `NavTestSeeker` had only validated
        the movement bridge and Crowds routing in isolation; this exercised
        the actual `Enemy` component's health/damage/death/pooling logic
        alongside DOTS movement for the first time.
      - **Bug #1**: `AgentCrowdPathingAuthoring.Group` wiring in `OnEnable()`
        wasn't reliably sticking for naturally-spawned (not manually
        edit-time-spawned) enemies. Worked around at the time by also
        wiring it in `Start()`.
      - **Bug #2, found during Phase 6 rollout, root-caused Bug #1 too**:
        once real gameplay spawning hit a **fresh** (never-before-pooled)
        enemy, got a hard crash —
        `ArgumentException: A component with type:Unity.Transforms.LocalTransform
        has not been added to the entity`, thrown from `Enemy.OnEnable()`
        (`EntityManager.GetComponentData<LocalTransform>`), via
        `ObjectPoolManager.SpawnObject` → `Instantiate`. Root cause: **wrong
        assumption about Unity's component execution order.** Assumed
        "Awake() runs for every component on a GameObject before OnEnable()
        runs for any of them" (a real, reliable guarantee — but only
        *scene-wide*, for "all Awake before all Start"). For a single
        newly-`Instantiate()`d GameObject's own components, Unity actually
        interleaves **per component**: `Enemy.Awake()` → `Enemy.OnEnable()`
        → `AgentAuthoring.Awake()` → `AgentAuthoring.OnEnable()` → ... in
        component list order. Since `Enemy` predates the Agent components on
        these prefabs (appended later, at the end of the component list, by
        the setup tool), `Enemy.OnEnable()` genuinely runs *before*
        `AgentAuthoring.Awake()` has created `LocalTransform` on the entity
        at all — explaining both the crash (unguarded `GetComponentData`)
        and Bug #1 (the `Group` setter's `if (m_Entity == Entity.Null) return;`
        guard was silently no-op-ing for the same reason, on the same code
        path). Fixed properly this time: extracted the entity-touching
        logic into `RefreshAgentState()`, guarded on
        `EntityManager.HasComponent<LocalTransform>(entity)` so it's a
        harmless no-op when called too early, called from both `OnEnable()`
        (correct for pool *reuse*, since `AgentAuthoring.Awake()` already
        ran during the object's original first-ever activation and never
        runs again) and `Start()` (scene-wide "after everything's
        Awake+OnEnable" *is* a reliable guarantee, so this is what actually
        completes the wiring for a fresh spawn — same mechanism that
        incidentally made the Bug #1 workaround "work").
      - **Bug #3, same class of bug, different component, found on *pool
        reuse* this time** (2026-07-09): `ArgumentException: A component
        with type:ProjectDawn.Navigation.AgentCrowdPath has not been added
        to the entity`, from `AgentCrowdPathingAuthoring.set_Group` →
        `Enemy.RefreshAgentState` → `Enemy.OnEnable` → `ObjectPoolManager.SpawnObject`'s
        *reuse* branch (`SetActive(true)` on a previously-pooled enemy, not
        a fresh `Instantiate`). Cause: `AgentCrowdPath` isn't like
        `LocalTransform`/`AgentBody` (added once in `Awake()`, never
        removed) — `AgentCrowdPathingAuthoring`'s own `OnDisable()`/`OnEnable()`
        **remove and re-add** it on every activation cycle. Since `Enemy`
        still precedes the Agent components in list order, `Enemy.OnEnable()`
        → `RefreshAgentState()` ran before `AgentCrowdPathingAuthoring.OnEnable()`
        had re-added the component *this* cycle — `SetSharedComponent`
        (unlike `AddSharedComponent`) throws if the component isn't already
        present, unlike the `Entity.Null` check the `Group` setter itself
        already guards against. Fix: guarded the `Group` assignment on
        `pathing.HasEntityPath` (same defensive pattern as the
        `LocalTransform` guard). Also realized while fixing this that
        `Enemy.cs` doesn't actually need to re-set `Group` on every pool
        reactivation at all — `m_Group` is a plain field on the component
        *instance*, not ECS data, so once `Start()` sets it correctly on
        first-ever activation, it persists on that instance for its whole
        lifetime, and `AgentCrowdPathingAuthoring.OnEnable()`'s own
        (unmodified, package) code already correctly restores
        `AgentCrowdPath` from it on every later reactivation with no help
        needed — the `OnEnable()`-time attempt in `Enemy.cs` was always
        redundant for reuse, just harmful once it started throwing instead
        of silently no-op-ing.
- [x] Phase 5 — Integrate with `ObjectPoolManager` lifecycle. Substantially
      covered by Phase 4's `OnEnable()` work (position resync, `Stop()`,
      speed reset) — confirmed working as part of the same 2026-07-09
      playtest (real spawns go through the pool).
- [ ] Phase 6 — **In progress.** Roll out to all remaining enemy prefabs.
      Confirmed by tracing class inheritance (`grep`, not guesswork) that
      25 prefabs total use `Enemy`/`Slime`/`FlyingEnemy` (including
      `MegaBoss Reaper.prefab`, which extends `FlyingEnemy` for its chase
      behavior on top of its own projectile-firing logic — easy to miss
      since its prefab's root script reference is `MegaBoss.cs`, not
      `FlyingEnemy.cs`/`Enemy.cs` directly). `Green Cube` already done,
      24 remain:
      - Ground (14): Blue Cube, Blue Flame, Boss Slime, Brown Goop Guy,
        Grey Ogre, Grey Ogre Boss, Mini Mushroom, Mummy, Mummy Boss,
        Mushroom Boss, Red Cube, Red Flame, Red Goop Guy, Slime
      - Flying (8): Bat, Small Bat, Dark Ghost, Ghost, One Eye, Three Eye,
        EvilHelicopter, TurnipHelicopter
      - Root-level (2): Tree Stump, MegaBoss Reaper
      - Run **Tools ▸ Navigation ▸ Add Agent Navigation To Selected
        Prefabs** once across all 24 (multi-selection supported). Since
      `Enemy.cs`'s `_agent` branch falls back to the old movement path for
      un-migrated prefabs, this can happen incrementally, testing each
      batch before moving to the next, rather than one big cutover.
      **Optional follow-up, not required to finish Phase 6** — user
      reported performance already looks strong without it, but still
      worth doing since it removes the exact redundant Box2D solving cost
      that originally motivated this whole migration: disable Enemy-vs-Enemy
      and Enemy-vs-FlyingEnemy in **Edit ▸ Project Settings ▸ Physics 2D ▸
      Layer Collision Matrix** (currently the Unity default — every layer
      collides with every other). Deliberately not hand-edited directly in
      `ProjectSettings/Physics2DSettings.asset` — its `m_LayerCollisionMatrix`
      is a dense packed bitmask with per-layer bit boundaries not confirmed
      precisely enough to safely hand-edit blind; a wrong edit could
      silently disable the wrong layer pair (e.g. the player damage trigger
      or Enemy-vs-Wall). Two checkbox clicks in the Editor UI instead.
- [ ] Phase 7 — Performance pass at high enemy counts (hundreds, matching
      `gameSupervisorController`'s late-game spawn rates); compare against
      current baseline; tune `AgentSeparation` radius/weight and consider
      `AgentSpatialPartitioning` settings if present.
- [ ] Phase 8 — Cleanup: remove now-dead code (`Vector3.MoveTowards` path);
      `Rigidbody2D`/`CircleCollider2D` stay (§4a tier A keeps them for wall
      collision + player damage trigger) — just confirm the layer collision
      matrix change (Enemy-vs-Enemy disabled) actually landed and stuck.
      Update this doc to "Done" status.

### Next steps for your Editor session (finalize §4b, then start Phase 4)

Wall routing is confirmed working (§4b bug log). What's left is making the
test setup permanent and cleaning up test artifacts, before Phase 4 (wiring
the real `Enemy.cs`) starts:

1. **Save the scene now.** The `Crowd Surface (Walls)` / `Crowd Group
   (Enemies)` / `Crowd Wall Baker` GameObjects created by **Tools ▸
   Navigation ▸ Setup Crowd Surface In Open Scene** were deliberately left
   unsaved during testing (§4b's original instructions) in case something
   was wrong — now that routing is confirmed, they should become a
   permanent part of `Game.unity`. `randomSeed` on `Walls.asset` is back to
   `1` (true) — confirm a fresh Play session still generates a new random
   layout and `CrowdWallBaker` still logs `Stamped N wall cell(s)...` for
   it (this exercises the "re-bake on every random regenerate" path for
   the first time, not just the fixed-seed layout used during debugging).
2. Delete the "NAV TEST - Green Cube (delete me)" GameObject — it's a
   smoke-test artifact, not meant to ship in the scene.
3. Let me know once both are done and I'll start Phase 4: replacing
   `Enemy.Move()`'s `Vector3.MoveTowards` with the real `AgentAuthoring`
   wiring (mirroring what `NavTestSeeker` proved out), including the
   `GlobalReferences.crowdGroup` timing note from the §4b bug log (needs
   checking against the `ObjectPoolManager` pooling lifecycle, Phase 5).

## 8. Risks / things that could blow up the plan

- **Resolved**: wall fidelity was originally tier (A) (§4a), then
  superseded by Continuum Crowds (§4b) after tier (A) tested "clunky" —
  confirmed working end-to-end, no longer an open risk.
- Object pooling + entity lifecycle mismatch (destroyed/recreated entities
  vs `SetEnabled` toggling) — confirmed a real issue, not just theoretical
  (§7 Phase 4 bug log): don't assume "Awake() before OnEnable(), for every
  component on a GameObject" the way "Awake() before Start(), scene-wide"
  is safely assumed — Unity only guarantees the latter. For a single
  newly-instantiated GameObject, component initialization interleaves
  per-component in list order, so code added to a component that was
  appended *after* an existing one (exactly what the setup tool does,
  appending Agent components after the pre-existing `Enemy`) can't safely
  touch another just-appended component's entity data from `OnEnable()`
  without either guarding for "not ready yet" or deferring to `Start()`.
  Applies to any future runtime component-touches-another-component's-ECS-data
  code in this codebase, not just `Enemy.cs`.
- No existing DOTS usage in this project at all — first-time World
  bootstrap issues (e.g. domain reload behavior, `World.DefaultGameObjectInjectionWorld`
  timing relative to `gameSupervisorController.Start()`) may surface that
  don't show up in Age-of-Sprites since it's DOTS-native from the start.
  This project’s `SpawnObject` also does not currently wait a frame after
  `Instantiate`, so `AgentAuthoring.Awake()` timing relative to
  `SpawnObject`’s subsequent `transform.position` set needs checking — right
  now pooled reactivation sets `transform.position` directly on the
  GameObject *before* re-enabling, which is fine, but on first-ever
  `Instantiate` `Awake()` runs before the position is set, so the entity’s
  initial `LocalTransform.Position` may briefly be wrong for one frame.
- Prefab editing at scale (~15+ enemy prefabs) — worth scripting the
  component addition (`MenuItem` batch tool or `PrefabUtility` script)
  rather than doing it by hand in the inspector 15 times.

## 9. Adjacent work: XP orb pickup performance (2026-07-09)

Not part of the enemy migration itself, but surfaced during Phase 6
playtesting and worth logging here for continuity — a huge frame spike
(66ms/15fps observed, later 43% of a frame in `Physics2D.FindNewContacts`
alone) on picking up a large cluster of accumulated XP orbs.

- Diagnosed via Unity Profiler CPU breakdown: dominated by
  `Physics2D.FindNewContacts`/`ContactsCollide`/`CompileContactCallbacks`/
  `TriggerContactsFinalUpdate` — Box2D broad/narrow-phase contact
  resolution, not script cost. "Calls: 6" in the profiler = 6 fixed-timestep
  physics steps ran within one rendered frame (catch-up steps after the
  frame already ran long — a self-reinforcing spiral).
- Root cause: `Experience.prefab`'s `CircleCollider2D` (radius ~0.064,
  trigger) had `IncludeLayers`/`ExcludeLayers` both at `0` — no
  restriction — so every XP orb ran full Box2D broad-phase against *every*
  layer, including every *other* XP orb (all on Default layer 0), even
  though `EnemyXpObjectBehaviour` only ever acts on 2 layers
  (`Experience`=8, magnet range; `Player`=12, pickup). With potentially
  hundreds of orbs clustered at a kill site, that's near-`O(n²)` wasted
  self-collision checking for zero gameplay purpose.
- First fix — `IncludeLayers` restricted to `{8, 12}` (bitmask `4352`) —
  measurably helped narrow-phase (`ContactsCollide` dropped ~45ms→27ms) but
  broad-phase (`FindNewContacts`) got *worse* in a bigger retest
  (32ms→225ms). Confirmed why: Unity/Box2D's broad-phase AABB-overlap
  detection is purely spatial, not layer-aware — layer filtering only
  affects whether a *found* pair becomes an actual contact, not whether the
  spatial query considers the pair in the first place. With enough
  colliders densely clustered in one spot, broad-phase cost dominates
  regardless of layer restriction.
- Real fix: removed Collider2D-based detection for XP orbs entirely.
  `EnemyXpObjectBehaviour._circleCollider` is now disabled in `OnEnable()`
  (never generates Physics2D events, not part of broad-phase at all).
  `EnemyXpObjectManager` maintains a `List<EnemyXpObjectBehaviour>` of
  active orbs (swap-remove via each orb's `ActiveIndex`, O(1) even when a
  mass-pickup burst unregisters many at once) and calls a new
  `TickProximity(GameObject player)` on each once per frame — a plain
  squared-distance check against `magnetRange`/`pickupRange`, replacing
  what the two former trigger collisions did. `Rigidbody2D` stays (the
  one-time death-knockback `AddForce` impulse still needs a `Dynamic`
  body); only collider-based *detection* was removed.
- `magnetRange` (0.5) was confirmed directly from the player's "Experience
  Circle Collider" child object's radius. `pickupRange` (0.15, tunable in
  the Inspector) is an approximation — the original pickup-detection
  collider lived behind a nested-prefab reference (`fileID` override in
  `Game.unity` pointing at a `guid` that didn't resolve to any object
  findable via direct text search in `Player.prefab`) that couldn't be
  pinned down without opening the Editor. Worth a quick in-editor feel
  check.
- `InteractableLoot`'s own trigger-based `OnTriggerEnter2D` was left
  completely untouched — `BagController`/`BagItem` (loot bags, far lower
  volume than XP orbs) still use it normally; only `EnemyXpObjectBehaviour`
  bypasses it (via the now-disabled collider).
