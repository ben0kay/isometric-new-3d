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

---

**Scope of this document:** conventions and forward-looking decisions. It does not create the proposed world-generation or progression systems by itself.
