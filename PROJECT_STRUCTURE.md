# PROJECT STRUCTURE & CODING CONVENTIONS

> **Project:** Fractured Horizons — 3D isometric prototype (Godot 4 / C#)  
> **Purpose:** A living reference for where files belong, how systems communicate, and how to grow the game without repeating early 2D architectural problems.  
> **Status:** Conventions and roadmap, **not** a claim that future systems already exist.

## 1. Main rule: CAPITALS for major umbrella folders

All **top-level gameplay umbrellas** use `ALL_CAPS`. Examples: `PLAYER/`, `WORLD/`, `CAMERA/`, `VISUALS/`. Each umbrella owns one cohesive subject area.

- Keep the project root uncluttered: Godot project files, README, and broad architecture documents belong here.
- Nested subfolders use **descriptive PascalCase** (`PLAYER/Movement/`, `WORLD/Terrain/`, `VISUALS/Lighting/`). Do not force every nested folder into uppercase.
- Name scripts, resources, and scenes after their real purpose, not generic terms such as `Manager2` or `Stuff`.
- Put the code where its **responsibility** belongs, not merely where another script happens to call it.
- Add new top-level umbrellas only when the system becomes significant; avoid empty speculative folder trees.

## 2. Master config in EVERY umbrella

**Naming convention:** `<UmbrellaName>Config.cs`, directly inside its umbrella, with a corresponding `<UmbrellaName>Config.tres` when the config is used as a Godot Resource.

| Umbrella | Master config | Owns |
| --- | --- | --- |
| `CORE/` | `CoreConfig.cs` | Truly shared engine/game defaults and cross-cutting tuning |
| `PLAYER/` | `PlayerConfig.cs` + `.tres` | Movement, sprint, jump, facing, player action-key and mouse bindings |
| `CAMERA/` | `CameraConfig.cs` + `.tres` | Camera angle, azimuth, orthographic size, follow smoothing |
| `WORLD/` | `WorldConfig.cs` + `.tres` | World size/mode, seed, terrain and generation tuning, streaming budgets (as developed) |
| `VISUALS/` | `VisualConfig.cs` + `.tres` | Lighting, shadow quality, atmosphere and shared visual tuning |
| `DEBUG/` | `DebugConfig.cs` | Debug toggles, overlays, diagnostics when needed |
| Future umbrella | `<Name>Config.cs` + `.tres` | Settings relevant to that feature only |

**Current repo note:** `CORE/` and `DEBUG/` currently have config classes, but no `.tres` yet. They should receive resource files when a system actually consumes their settings. `NOTES/` is documentation, **not** a runtime gameplay umbrella and needs no config.

Master configs are **entry points for common tuning**, not compulsory giant files. A complex subsystem may also have focused config/definition resources in its own subfolders. For example, a future `BiomeDefinition.tres` belongs alongside biomes, not as dozens of unrelated properties inside `WorldConfig`.

### Config design rules

1. **One source of truth:** each setting has one owner. Avoid competing hardcoded defaults, duplicate scene overrides, and multiple 'master' values.
2. Use `[GlobalClass]` + `Resource` and `[Export]` / `[ExportGroup]` for inspector-friendly settings where suitable. The saved `.tres` is authoritative; code supplies sensible defaults.
3. **Configuration is not state:** movement speed is config; current health, player position, damaged walls and discovered POIs are runtime/save data.
4. Prefer meaningful units in names (`ChunkSizeMeters`, `SunElevationDegrees`) and clamp/validate values at their system boundaries.
5. Changes to creation-time settings (seed, world bounds, chunk dimensions, major POI blueprint) require a **new world** or explicit migration; runtime/quality settings may be adjustable without regeneration.
6. New major umbrellas should receive a master config on creation, even if initially empty. Avoid adding hundreds of unused exported properties.
7. Record config ownership and new important decisions here or in a focused design document.

## 3. Current project ownership (real folders)

```text
isometric-new-3d/
├── CORE/                      shared foundations, CoreConfig.cs
├── PLAYER/                    PlayerConfig.cs/.tres
│   ├── Movement/              input mapping and player physics
│   └── Visuals/               character scenes, rig, animation, Blender sources
├── CAMERA/                    CameraConfig.cs/.tres, independent follow camera
├── WORLD/                     WorldConfig.cs/.tres, World.tscn
│   └── Terrain/               temporary test ground; future terrain runtime
├── VISUALS/                   VisualConfig.cs/.tres
│   └── Lighting/              sun and environment presentation
├── DEBUG/                     DebugConfig.cs; future profiling tools
├── NOTES/                     plans and feature notes (no master config)
├── PROJECT_STRUCTURE.md       this root guide
├── README.md                  project overview and how to run
├── project.godot              Godot settings; main scene, rendering, etc.
└── isometric-new-3D.csproj    .NET project
```

**Player animation currently in progress:** changes to `PLAYER/Visuals/` should remain isolated from world-generation work. Do not silently change a player rig or model import pipeline while building world systems.

## 4. Future umbrellas (only add when implemented)

Possible future peers: `ENTITIES/`, `COMBAT/`, `ITEMS/`, `SYSTEMS/`, `UI/`, `AUDIO/`. When introduced, each gets its own `EntityConfig`, `CombatConfig`, etc., and corresponding `.tres` as appropriate. Name the umbrella config **consistently** and record its owner.

- `ENTITIES/`: creature/robot definitions, state, AI, spawning and presentation adapters.
- `COMBAT/`: damage, attacks, projectiles, hit detection, combat rules.
- `ITEMS/`: item definitions, inventory-related shared data.
- `SYSTEMS/`: cross-domain services such as saving, crafting, harvesting and interactions; give individual complex subsystems focused resources too.
- `UI/`: HUD and menus only, without owning core gameplay rules.
- `AUDIO/`: sounds, routing, audio catalogs.
- `WORLD/`: geography, terrain, biome sampling, caves, landmarks and chunk streaming; not player animations or combat rules.

A domain folder should not become a dumping ground. If `SYSTEMS/` gets too broad, promote a genuinely independent domain to its own uppercase umbrella.

## 5. Lessons from isometric-new-2d

The 2D project already separates `PLAYER/`, `WORLD/`, `ENTITIES/`, `COMBAT/`, `ITEMS/`, `SYSTEMS/`, `VISUALS/`, `UI/`, `AUDIO/` and `DEBUG/`. Reuse this **ownership idea**, not the 2D node types or entire scripts.

| From 2D | 3D improvement |
| --- | --- |
| A coordinating `Player.cs` calls numerous systems | Keep `Player.cs` thin; input, locomotion and visual/animation logic stay in dedicated files |
| `WorldGenerator` and chunk controller exist separately | Separate pure world sampling/planning from chunk mesh/collision lifetime even more strictly |
| Cave generation + cave chunk controller + world layer connections | Preserve the useful split between cave layout, streaming and transitions, but design for real 3D volume |
| Biome/resource/creature definitions | Use data-driven definitions independent of meshes and scene instancing |
| Save data is separated into sections | Keep a stable world blueprint plus incremental edits; serialize data, never whole scene trees |
| Performance and debug utilities are centralized | Establish generation budgets, loaded-chunk counters and reproducible seed tests early |

**Specific 2D references:** `WORLD/Chunks/ChunkController.cs`, `WORLD/Generation/WorldGenerator.cs`, `WORLD/Generation/Caves/`, `WORLD/Layers/`, `ENTITIES/Core/`, `SYSTEMS/Saving/`, and `NOTES/CodePreferences.md`.

## 6. World foundation principles (proposed, not implemented)

- **Very large finite procedural map** by default, but use a reusable boundary policy so a world can be configured as finite or unbounded without rewriting local terrain sampling.
- Determine terrain and biome from **seed + absolute world coordinates**, independent of the order chunks are loaded. World-size enforcement belongs to bounds/streaming rather than arbitrary per-chunk randomness.
- Plan a **lightweight global world blueprint** at new-game creation: guaranteed start zone, reserved significant landmarks/fortresses, progression-critical resources, cave regions and **exactly one endgame lair**.
- Global blueprint is data, not a fully loaded Godot scene. Only nearby chunks turn it into physical mesh, collision, vegetation, robots and creatures.
- Separate `WorldPlanning` (macro geography and guaranteed POIs), `Generation` (deterministic samplers), `Chunks` (runtime loading), `Caves` (underground geometry), and `Persistence` (changes and discoveries) when implemented.
- Surface terrain may use height-based meshes; future caves/overhangs need volumetric or custom mesh geometry. Do not encode the entire world as one height per X/Z forever.
- Design large-world coordinates deliberately before supporting hundreds of kilometres: numeric precision, floating origin or equivalent, stable chunk IDs and saving in logical coordinates.
- **World creation settings are snapshotted into the save.** Enlarging boundaries may be supportable; shrinking/changing seed requires explicit policy and migration.
- Do not make world content dependent on the current camera viewing distance. Rendering radius and simulation radius are separate concerns.

## 7. Scene and code rules

- Prefer **composition**: compact coordinating nodes call dedicated components.
- Gameplay simulation should not depend on UI, visual effects or camera nodes unless a narrow adapter is intentionally used (such as camera-relative control).
- Prefer data definitions/resources for shared content. Instances hold transient physical state; saves retain only required persistent differences from procedural defaults.
- Use explicit ownership and scene paths/references, avoid global singleton dependencies unless truly global.
- Keep scene trees understandable; group related helpers beneath plain nodes rather than flattening every helper into the world root.
- Respect the project's coding preferences: short script-purpose comment at top; explanatory comments above methods with a `// =========================================================` separator; use `#region` for useful groups; readability, reuse and performance over needless abstraction.
- Treat the 2D repo as an **architectural reference**, not an automatic source of up-to-date implementation requirements.
- Before editing active work (especially `PLAYER/Visuals/`), inspect the latest GitHub head and avoid overwriting concurrent Codex changes.
- Prefer limited, reviewable commits, and document substantive changes to folder ownership and configuration.

## 8. Development checklist for each new system

- [ ] Does it have a clear umbrella owner?
- [ ] Is its main tuning in the umbrella master config, with detailed definitions kept close to their domain?
- [ ] Is configuration separate from state and save data?
- [ ] Is it independent of 2D-only assumptions and unrelated presentation logic?
- [ ] Does it have a scene/component boundary rather than one enormous controller?
- [ ] Is procedural output reproducible for the same seed/coordinates?
- [ ] What happens when it unloads, reloads, or a save is restored?
- [ ] Can its performance be measured and tested in isolation?
- [ ] Did we update documentation when changing architecture?


## 9. Terrain generation & chunk streaming — next implementation

**Goal:** Replace the flat test ground with real, walkable 3D terrain that supports mountains, valleys and slopes. The isometric camera continues to follow the player's actual Y elevation. Work within `WORLD/`, not a new root-level `TERRAIN/` umbrella. This section is a **plan** and does not claim these files already exist.

### Proposed focused folder layout

```text
WORLD/
├── WorldConfig.cs                  # Umbrella master config (already exists)
├── WorldConfig.tres                # Editable resource (already exists)
├── World.tscn                      # World composition (already exists)
├── Coordinates/
│   ├── WorldCoordinates.cs        # Stable logical/world-space conversions
│   └── ChunkCoordinates.cs        # Signed chunk indices, including negatives
├── Boundaries/
│   └── WorldBounds.cs             # Single authoritative finite/unbounded policy
├── Generation/
│   ├── WorldSeed.cs               # Seeded deterministic random streams
│   ├── WorldGenerator.cs          # Coordinates generation services
│   └── Terrain/
│       ├── HeightSampler.cs       # Pure elevation sampling in X/Z
│       └── TerrainGenerator.cs    # Chunk terrain data generation
├── Chunks/
│   ├── ChunkManager.cs            # Requests, activates and retires chunks
│   ├── TerrainChunk.cs            # Runtime chunk node/ownership
│   └── ChunkLifecycle.cs          # Explicit loading/ready/retirement states
├── Terrain/
│   ├── TerrainMeshBuilder.cs      # Mesh from sampled heights
│   ├── TerrainCollisionBuilder.cs # Matching collision geometry
│   └── TestGround.tscn            # Existing test scene; retire only after replacement works
└── Persistence/
    └── WorldChangeStore.cs        # Future overlay of user-modified chunk data
```

These filenames are a **suggested ownership map**, not a request to generate numerous placeholder scripts. Codex should create files as needed, keep the implementation simple, and preserve `World.tscn`, `PLAYER/` and `CAMERA/` unless integration explicitly requires small changes.

### Master `WorldConfig` — future groups

- **World boundaries:** `FiniteWorld` (true by default), `WorldWidthKm`, `WorldLengthKm`. Provisional full-size target: **512 × 512 km**, NOT a commitment; start runtime tests with a **2 × 2 km** world.
- **Generation:** `WorldSeed` (a deterministic seed captured per new save), `ChunkSizeMeters` (candidate: **64 m**; lock per-save once released).
- **Terrain:** `TerrainHeightScale`, `TerrainFeatureScale`, optional slope and noise settings.
- **Streaming:** `LoadRadiusChunks`, `UnloadRadiusChunks` (unload must exceed load), per-frame generation/collision budgets.
- **Debug:** chunk outlines, seed display, loaded/queued chunk counters, regeneration diagnostics.
- **Existing prototype:** retain test-ground colour, grid, size and test blocks until the streamed ground replaces them.

Sample values are **initial tuning assumptions** to validate, not promises of performance. `WorldConfig.tres` should remain the authoritative resource rather than duplicating values across scene exports and scripts.

### Non-negotiable terrain rules

1. **Generation order independent.** A chunk produces the same geometry for the same saved seed/config and signed chunk coordinate, regardless of neighbour-load order.
2. **Seamless boundaries.** Sample heights using global coordinates so adjacent chunk edges match exactly; use consistent mesh grid resolution and collision.
3. **Real 3D physical height.** Ground meshes and collision share an elevation source. Keep player physics on `CharacterBody3D` and let the existing world-owned camera follow player Y naturally.
4. **Bounded streaming.** Schedule preparation and retirement around the player; unload distant chunk nodes, collision and temporary data. Never instantiate a 512 km world in memory.
5. **Separate data and presentation.** Pure height/biome samplers should not require Godot scene nodes; mesh/collision instantiation belongs to runtime builders.
6. **Stable coordinates.** Handle negative X/Z consistently (floor division rather than truncation) and plan for precision over hundreds of kilometres, including a floating-origin strategy before long-range tests.
7. **Clear finite edges.** A single `WorldBounds` answers whether a chunk is playable; an optional unbounded mode should reuse the same generator. Decide visual boundary treatment later.
8. **Streaming readiness.** Do not let a player fall through terrain merely because its collision chunk has not finished generating; protect the spawn area and prefetch movement direction.
9. **Do not tie everything to the camera.** Visual load radius, physics collision radius and simulation radius may be tuned independently.
10. **Future caves/overhangs:** heightfields are suitable for surface ground but cannot directly represent ceilings, tunnels or vertically stacked surfaces. Reserve an explicit custom-mesh/underground-region path.

### Terrain implementation passes for Codex

- **T1 — Config, coordinates and boundary policy.** Unit tests for positive/negative coordinates and chunk edges; preserve present gameplay.
- **T2 — Stream a grid of flat chunks.** Replace one flat surface only after safe spawn, seamless collision, loading and unloading are demonstrated.
- **T3 — Deterministic hills.** Real elevations, normals and matching collision; player climbs slopes; camera tracks elevation.
- **T4 — Visual quality and diagnostics.** Noise shaping, terrain LOD planning, time budgets, profiling and stress tests.
- **T5 — Biome and macro-landmark planning (later).** Seeded geographical regions, reserved POIs, guaranteed resources and exactly **one** primary endgame lair per world blueprint.

**Acceptance tests:** revisit the same chunk and get identical ground; cross seams without steps or holes; run around chunk boundaries without falling; climb and descend a hill; move beyond load radius and verify old chunks retire; reject out-of-bounds chunks; reproduce a sample from its seed. Do not claim a 512 km playtest until precision and streaming have actually been validated.

## 10. Saving, profiles, boot and world persistence — establish early

**Decision:** Introduce a minimal working save/profile/continue loop **before** populating the world with lots of harvestable resources, creatures and buildings. This avoids the expensive persistence retrofit encountered in 2D. Do **not** implement an entire finished save framework before basic terrain streaming works.

### Suggested future folders

```text
SYSTEMS/
├── SystemConfig.cs / SystemConfig.tres   # Umbrella master config when introduced
└── Saving/
    ├── SaveManager.cs                   # Save/load coordination
    ├── SaveManifest.cs                  # World identity, schema version, seed/settings snapshot
    ├── ProfileManager.cs                # User profiles; distinct from worlds
    └── SaveVersion.cs                   # Versioning and migration rules

UI/
├── UiConfig.cs / UiConfig.tres           # UI umbrella settings when introduced
└── Menus/
    ├── Boot/
    │   └── BootScreen.tscn              # Continue, New World, profiles, settings, quit
    └── Profiles/
        └── ProfileSelect.tscn

WORLD/
└── Persistence/
    └── WorldChangeStore.cs              # Per-world/per-chunk modifications, not terrain generation
```

`UI/` only presents choices. `SYSTEMS/Saving/` owns profile/world save operations. `WORLD/Persistence/` owns how world changes are represented and re-applied, but must not hard-wire UI menus into terrain.

### Profile, world and save data boundaries

- **Profile:** player identity and selection; a profile may own multiple distinct worlds/save slots later.
- **World manifest:** persistent world ID, generation seed, generator/version identity, bounds, chunk geometry settings, selected blueprint and creation-time settings snapshot.
- **Player state:** location in **stable logical coordinates**, including 3D elevation, plus future vitals/inventory.
- **World blueprint:** planned geography/important POIs saved once on creation; fixed positions must not silently relocate after a generator update.
- **Changes overlay:** record only persistent differences from procedural defaults (e.g. harvested resource, destroyed robot, built base, modified terrain), keyed by stable world and chunk/object IDs.
- **Transient runtime:** instantiated nodes, meshes, collision shapes, caches and visible chunks are regenerated and not serialized as full Godot scene trees.

**Restore flow:** load saved manifest/config and player state → generate baseline terrain from seed at the needed chunks → reapply persistent changes → activate colliders/entities → allow gameplay. Terrain generation must work without directly reading the save manager.

### Persistence safeguards from day one

- Use an explicit schema version and migration strategy.
- Write to a temporary file and replace atomically where practical; keep a recovery backup. Never corrupt the sole save on interrupted writing.
- Validate profile IDs, world IDs, seed/settings and coordinates before loading. Keep old saves readable or explain incompatibility.
- Avoid excessive per-frame writes; batch dirty chunk changes and commit on explicit save/periodic safe checkpoints.
- Stable generated IDs must depend on world ID/seed, chunk coordinates and deterministic object identity, not runtime instance IDs.
- When chunk nodes unload, persistent modifications must survive. Separate retirement of nodes from retention of dirty/save data.
- World shrinking, changes to seed, terrain scale, chunk size or generation rules require a migration/new-world policy. Expanding bounds may be supportable if planned carefully.
- Save files belong in Godot's `user://`, not `res://` or the GitHub repository. Document save paths for testing.

### Save implementation passes for Codex

- **S1 — Manifest/data contracts:** schema version, profile ID, world ID, seed, finite bounds/settings snapshot, player logical position, testable serialization.
- **S2 — Basic profile + boot flow:** new profile, new world, continue, save and quit; gameplay launched only with valid world context.
- **S3 — Stable player save/restore:** player position persists across a restart, with safe terrain spawn readiness.
- **S4 — Chunk modification persistence:** start with one test object or simple persistent edit; unload/reload and restart retain changes.
- **S5 — Expansion later:** inventory, constructed bases, harvested deposits, creature state, caves and evolving world events.

**Acceptance tests:** create two profiles/worlds without sharing states; save and restart at the same position; travel away/back and get the same terrain; prevent invalid/corrupt saves from crashing; resume safely while terrain collision prepares.

## 11. Agreed sequencing and handoff expectations

1. Let the ongoing Codex work in `PLAYER/Visuals/` complete independently.
2. Build **T1–T3**: world coordinates, boundaries, flat chunk streaming, then procedural elevation.
3. Implement **S1–S3**: persistent new-world identity, a minimal boot/profile UI and save/continue.
4. Add world-edit persistence (**S4**) before serious harvesting/building systems.
5. Expand to biomes, geological regions, caves, robot/creature population, POI planning and endgame progression once foundations are stable.

For any Codex task: read this root guide and `NOTES/CodePreferences.md`, inspect the live repository head, work in a focused commit, and **do not modify active player animation sources** unless required and explicitly coordinated. Keep generated/editor files where Godot needs them, avoid unnecessary renames, and report tested versus untested behaviour clearly.

**Note:** This section documents design direction only. It does not itself implement terrain generation, profile screens, or saving.


## 12. Biome definitions, specialized terrain and first Test biome

**Convention:** every authored biome has one matching folder under `WORLD/Generation/Biomes/Definitions/<BiomeName>/`. For example:
`WORLD/Generation/Biomes/Definitions/Test/Test.tres`.

```text
WORLD/Generation/
├── WorldGenerator.cs                      # Future world orchestration
├── Terrain/
│   ├── HeightSampler.cs                  # Future shared sampling interface
│   └── TerrainGenerator.cs               # Future world terrain coordinator
└── Biomes/
    ├── BiomeDefinition.cs                # Shared editable biome identity + terrain reference
    ├── BiomeCatalog.cs                   # Future enabled-biome collection
    ├── BiomeSampler.cs                   # Future biome selection/blending
    ├── TerrainProfile.cs                 # Shared editable terrain tuning resource
    └── Definitions/
        ├── Test/
        │   ├── Test.tres                  # Actual Test biome definition
        │   └── TestTerrain.tres           # Editable Test terrain profile
        └── RockyMountains/               # Example of a FUTURE biome
            ├── RockyMountains.tres
            ├── RockyMountainsTerrain.tres
            └── RockyMountainsTerrain.cs  # Only if shared terrain shaping is inadequate
```

### Biome ownership and per-biome control

- `BiomeDefinition` defines stable ID, display name, enabled flag and a reference to a terrain profile. It can later reference biome vegetation, resources, encounters, weather and landmark rules, preferably as focused resources rather than one enormous config.
- `TerrainProfile` is a reusable default data resource: base elevation, hill amplitude and noise feature scale. This is the **first** choice for Test and simple biome variations.
- A specialized biome-specific terrain script (e.g. `RockyMountainsTerrain.cs`) is **optional**. Use it only when generic profile parameters cannot describe the shape. Future terrain sampling should use a shared contract so bespoke implementations do not force branching on biome names.
- A biome's resource values are content definitions, not another umbrella master config. `WORLD/WorldConfig.tres` still owns world bounds, seed, streaming and a prototype forced-biome ID.
- `BiomeCatalog` will register all available biomes. `BiomeSampler` selects and blends them spatially. Chunk meshes and collision are generated by the shared world/chunk system, not by independent biome chunk managers.
- Heightfields must sample **absolute X/Z** coordinates for cross-chunk consistency. Biome transitions need smooth height blending to avoid sudden elevation seams. Caves/overhangs remain separately modeled as true 3D geometry.

### First terrain prototype

1. Use the Test biome as the **only active biome** and select it explicitly in the world-generation test mode.
2. Keep `Test.tres` and `TestTerrain.tres` in the same Test folder. Start with editable gentle hill height and scale, changing toward mountains only after seamless chunks work.
3. Codex should first wire up simple deterministic height sampling and shared mesh/collision builders using the Test profile. No biome-specific generator script is needed until shape complexity justifies one.
4. Verify the same seed/sample reproduces the same elevations, two chunk boundaries match, and the player and independent isometric camera physically rise along the slopes.
5. Later introduce catalog-based selection, distance/climate blending, specialized biome shaping, resources and populations.

**Implementation status:** Test biome resource classes and assets are scaffolded as definition data. They are **not connected** to terrain generation or `World.tscn` yet; Codex's terrain pass will integrate them. Do not mistake a resource file for a working procedural mesh.


## 13. World content: generic library + biome-exclusive assets

**Decision:** support both a reusable generic world-content library and content authored inside a specific biome. Folder ownership identifies where definitions/assets are maintained; **biome content profiles determine where they actually spawn**. A generic object is *eligible* for reuse, not automatically spawned everywhere.

### Target layout (future files are examples, not yet implemented)

```text
WORLD/
├── WorldConfig.cs
├── WorldConfig.tres
├── Generation/
│   ├── Biomes/
│   │   ├── BiomeDefinition.cs
│   │   ├── BiomeCatalog.cs                 # Future
│   │   ├── BiomeSampler.cs                 # Future
│   │   └── Definitions/
│   │       ├── Test/
│   │       │   ├── Test.tres               # Already scaffolded
│   │       │   ├── TestTerrain.tres        # Already scaffolded
│   │       │   └── Content/
│   │       │       └── TestContent.tres    # Future
│   │       ├── BasaltFlats/
│   │       │   ├── BasaltFlats.tres
│   │       │   ├── BasaltFlatsTerrain.tres
│   │       │   └── Content/
│   │       │       ├── BasaltFlatsContent.tres
│   │       │       ├── Rocks/
│   │       │       │   └── BasaltColumn/
│   │       │       │       ├── BasaltColumn.tres
│   │       │       │       └── BasaltColumn.tscn
│   │       │       └── Plants/
│   │       │           └── AshFrond/
│   │       │               ├── AshFrond.tres
│   │       │               └── AshFrond.tscn
│   │       └── RockyMountains/
│   │           ├── RockyMountains.tres
│   │           ├── RockyMountainsTerrain.tres
│   │           └── Content/
│   │               ├── RockyMountainsContent.tres
│   │               └── Rocks/
│   │                   └── CliffBoulder/
│   │                       ├── CliffBoulder.tres
│   │                       └── CliffBoulder.tscn
│   └── Placement/
│       └── PlacementRules.cs              # Shared environmental validation
├── Contents/
│   ├── Shared/
│   │   └── WorldObjectDefinition.cs      # Shared type/identity/scene reference
│   ├── Generic/
│   │   ├── GenericContentCatalog.tres     # Reusable catalog, not a universal spawner
│   │   ├── Rocks/
│   │   │   ├── SmallRock/
│   │   │   │   ├── SmallRock.tres
│   │   │   │   └── SmallRock.tscn
│   │   │   └── Boulder/
│   │   │       ├── Boulder.tres
│   │   │       └── Boulder.tscn
│   │   └── Plants/
│   │       └── CommonFrond/
│   │           ├── CommonFrond.tres
│   │           └── CommonFrond.tscn
│   └── Placement/
│       ├── WorldObjectSpawner.cs          # One shared deterministic placement system
│       └── WorldObjectPlacementProfile.cs # Resource describing density/constraints
└── Chunks/                                # Shared streaming/runtime ownership
```

### Ownership rules

1. **Generic library:** reusable rocks, plants, and eventually other resources are defined once in `WORLD/Contents/Generic/` and referenced by any number of biome content profiles.
2. **Biome-exclusive content:** truly biome-specific objects live with that biome under `Definitions/<Biome>/Content/<Category>/<ObjectName>/`. A matching `.tres` holds the definition and a `.tscn` holds the instance/presentation (with meshes/materials nearby as appropriate).
3. **No content duplication:** one `SmallRock.tres` should be referenced from multiple profiles, not copied into every biome. If an originally exclusive object is reused broadly, share its definition; move it only when useful, respecting Godot resource paths and UIDs.
4. **Biome content profile:** `TestContent.tres`, `BasaltFlatsContent.tres`, etc., hold *references* to placement entries for both generic and exclusive objects. Biomes opt in or out of every available object; generic does **not** mean universal.
5. **One shared spawner:** `WorldObjectSpawner` applies profiles per chunk, with deterministic seed/coordinates and stable generated object IDs. Avoid `BasaltFlatsRockSpawner.cs` / `TestRockSpawner.cs` duplicates.
6. **Shared placement rules:** each entry may tune density, grouping, spacing, slope, height/elevation, surface/biome restrictions and exclusion zones. Use a shared definition/entry type and optional specialized placement strategies only for genuinely different phenomena.
7. **Avoid double ownership:** `WORLD/Generation/Placement/` should hold reusable geographical checks/planning; `WORLD/Contents/Placement/` owns runtime prop spawn placement. If they overlap, consolidate instead of building two competing spawners.
8. **Save-awareness:** procedural placement is order-independent; removed/harvested objects remain removed when their chunk reloads using persistent overrides keyed by stable IDs. Chunk unloading should not rewrite procedural defaults.
9. **Global-config discipline:** `WORLD/WorldConfig.tres` owns seed/bounds/chunk streaming and high-level test-mode controls. Biome profiles own content densities and local rules. Do not put every rock and plant's settings in WorldConfig.
10. **Separation from terrain:** content placement reads sampled elevation/slope and biome identity, but terrain mesh generation does not depend on what plants happen to spawn.

### First end-to-end content test for Codex (AFTER terrain chunks work)

- Create `TestContent.tres` and reference it from `Test.tres`.
- Create one generic `SmallRock` definition plus placeholder scene.
- Create one Test-exclusive plant/rock definition under `Definitions/Test/Content/`.
- Use a single shared placement system to spawn both, under deterministic placement profiles.
- Verify identical placements after exiting and re-entering a chunk, across seam boundaries, and after relaunching with the same seed.
- Verify spawn density can be tuned independently for Test without modifying generic assets or affecting unrelated biomes.
- Only later add loot, harvesting, creatures or save mutations.

**Implementation status:** this section documents folder placement and future behaviour only. The generic catalog, content profiles and spawner are not created by this documentation update.

---

**Scope of this document:** project conventions, current ownership, and staged implementation plans. Proposed features are not implemented merely by documenting them.

## 14. First finite Test terrain implementation

T1–T3 are now implemented as a first prototype; runtime verification in Godot is still required.
World.tscn uses `WORLD/Chunks/ChunkManager.cs` instead of instantiating TestGround.
`WorldConfig.tres` references the existing Test biome, owns seed/finite dimensions/streaming, and starts at 2048 × 2048 m.
Biome terrain tuning remains in TestTerrain.tres. Shared HeightSampler, TerrainMeshBuilder and TerrainChunk provide baseline heights, mesh and matching collision.
WorldBounds and signed ChunkCoordinates own finite indexing. WorldGenerationSettings under Persistence captures versioned creation inputs for future saves.
No disk persistence is implemented in this pass; S1–S3 remain next. Generic/exclusive content spawning remains later.
See `WORLD/TERRAIN_PROTOTYPE.md` for tuning, limitations, save integration and executable checks.

## 15. Test biome content and debug HUD

`DEBUG/Hud.cs` reads player X/Y/Z, active biome ID, chunk coordinate and loaded chunk count.
`DebugConfig.tres` controls visibility and refresh interval.
Test biome assets live in `Definitions/Test/Content/ObjectsTest/`; `TestContent.tres` configures the unique `TestContentGeneration.cs` strategy.
The strategy implements `WORLD/Contents/Placement/WorldContentGeneration.cs`; shared ChunkManager owns lifecycle and invokes the biome reference.
This is biome-exclusive placeholder content only; the generic content catalog remains future work.
Placement IDs are deterministic, but persistence overlays and placement-settings snapshots remain future saving work.
