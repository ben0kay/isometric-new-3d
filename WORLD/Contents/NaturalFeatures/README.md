# Natural Features — shared rocks, trees, plants and ground cover

**Status: architecture proposal only.** This folder currently holds this README. The classes, subfolders and resources described below are **planned**, not implemented. The current Test biome still uses its own placeholder object scenes and `TestContentGeneration`.

## Purpose

`WORLD/Contents/NaturalFeatures/` is the home for **reusable, naturally occurring 3D world objects**, both biological and geological. "Vegetation" would exclude rocks, and "WorldContent" would also include crates, buildings and machines. "NaturalFeatures" covers both without confusing natural environment props with constructed content.

| Family | Examples | Expected behavior |
| --- | --- | --- |
| `Rocks/` | Basalt boulders, stone clusters, mineral outcrops, jagged formations | Usually solid; some can be mined |
| `Trees/` | Alien trees, dead trunks, enormous fungal/tree-like structures | Trunk collision; optional harvesting and wind |
| `Plants/` | Fronds, alien shrubs, mushrooms, flowers, resin-bearing growth | Usually walkable or small collision; optional harvesting |
| `Grass/` | Ground tufts, sparse reeds, dry alien ground cover | Primarily visual, lightweight, often no collision |
| *Future families* | Crystals, corals, spires, unusual alien formations | Add a family only when its behavior warrants one |

For the **initial Test biome**, prioritize **barren basalt rocks and weathered geological forms**, with sparse strange alien plants/dead growth at most. No lush grassy landscape by default.

## Proposed folder layout

```text
WORLD/
  Contents/
    NaturalFeatures/
      README.md
      NaturalFeatureConfig.cs               # Planned; local defaults/limits
      NaturalFeatureConfig.tres             # Planned; Inspector configuration
      Shared/
        NaturalFeatureDefinition.cs         # Planned; common identity/visual/placement data
        NaturalFeatureVariant.cs            # Planned; weighted mesh/scene variants
        NaturalFeature3D.cs                 # Planned; optional interactive instance
      Rocks/
        RockDefinition.cs                   # Planned; rock-specific settings
        Definitions/
          BasaltBoulder/
            BasaltBoulder.tres
            Models/
      Trees/
        TreeDefinition.cs
        Definitions/
      Plants/
        PlantDefinition.cs
        Definitions/
      Grass/
        GrassDefinition.cs
        Definitions/
```

This is a **target organization**, not a request to create empty files or duplicate a manager for each family. Keep species assets and variant models together. If biome-specific imported assets are genuinely exclusive to one biome, its `WORLD/Generation/Biomes/Definitions/<Biome>/Content/` may reference them without moving shared behavior into that biome.

## Separation of responsibilities

**1. Definitions (`NaturalFeatures/`) — what an object is.** Shared Godot `Resource` definitions (`.tres`) should hold stable species IDs, display names, model/scene references, weighted variants, scale ranges, approximate footprint, collision/interaction capabilities and optional references to harvesting profiles. Reuse a definition across multiple biomes. Make actual values editable in the Inspector; do not hard-code per-species tuning in spawners.

**2. Biomes (`WORLD/Generation/Biomes/`) — what may appear here.** Each biome selects allowed natural-feature species, per-species density/weights and biome overrides such as slope range, altitude, spacing, exposure or geological region. The barren Test biome must be able to disable grass/trees altogether while still spawning rock families. Biome-specific generation assets stay with the biome; shared definitions stay here.

**3. Placement (`WORLD/Contents/Placement/`) — where objects appear.** Continue using the existing `WorldContentGeneration.Populate(parent, chunk, settings, terrain)` contract. `ChunkManager` remains the **sole owner of terrain chunk lifetimes**; placement must not create a second chunk manager. Eventually factor reusable placement helpers out of the Test-specific `TestContentGeneration.cs` without blindly replacing working placeholder content. Use deterministic world-seed + signed chunk coordinates + stable candidate IDs; test slope and terrain height in world space. Respect collisions, ground contact, clearances and the player starting zone.

**4. Instances — what happens while loaded.** A rock or tree may instantiate a lightweight `Node3D`/`StaticBody3D` scene for physics and interaction. Static decoration need not have a per-frame script. Use real 3D meshes/models, 3D rotations, physically appropriate collider shapes and a consistent origin at ground contact; do not port 2D sprite footprints or rendering systems literally.

**5. Harvesting, loot and items — what is obtained.** Item definitions belong in `ITEMS/`; harvesting logic and drop tables belong in future `SYSTEMS/Harvesting/` and `SYSTEMS/Loot/`. A natural feature can **reference** a harvest/drop profile, but the rock/tree definition should not contain a hard-coded inventory or drop implementation. Decorative and harvestable examples can share the same visual family.

**6. Persistence — what changed permanently.** Baseline generation is deterministic, but removal, mining depletion, growth or destruction must be stored as changes keyed by a stable campaign/world ID and generated object ID. The current Test system writes a `generated_id` metadata value but does **not** persist removed objects. Future persistence must prevent mined rocks from reappearing when chunks unload/reload or a campaign is continued. Avoid using runtime node instance IDs.

## 3D and streaming design rules

- **Chunk-owned lifetime:** Content lives under each terrain chunk's existing `Content` node; unloading a chunk releases its runtime feature instances, while save-state overlays survive.
- **Continuous placement:** Sample height and slope from existing 3D terrain services. Avoid floating or buried roots and visible artificial grid alignment.
- **Collisions by function:** Boulders and tree trunks generally block movement; fronds and small plants may not; individual grass blades should not have physics bodies.
- **Variation:** Define explicit mesh/scene variants with weights, deterministic yaw, restrained scale variation and optional biome tinting. Match collider/footprint to the chosen variant.
- **Performance:** Do not create an independent `_Process` or `_PhysicsProcess` per blade/rock. Dense grass and tiny ground clutter should eventually use batching (`MultiMeshInstance3D`), culling/distance tiers and inexpensive wind materials when appropriate. Reserve standalone nodes for interactable objects. Do not mandate LOD for the first boulder prototype, but make it possible later.
- **Visuals:** Keep reusable rendering/shaders in `VISUALS/`; species definitions may reference meshes/materials. Keep the material palette compatible with the harsh basalt Test biome.
- **Future editors:** Prefer serializable `.tres` definitions and stable IDs so a future external Toolbox can discover species, inspect variant assets, edit placement/harvest settings and validate references through Godot-backed import/export. Do not create a competing JSON runtime database.

## Reference: older 2D project

Study **`ben0kay/isometric-new-2d`** for its *data and gameplay architecture*, not its 2D rendering:

- `WORLD/Contents/Rocks/` — `RockDefinition.cs`, `RockSpawner.cs`, `LargeRock/` variants and harvesting configuration
- `WORLD/Contents/Vegetation/Trees/` — `TreeDefinition.cs`, tree species resources
- `WORLD/Contents/Vegetation/Plants/` — `PlantDefinition.cs`, alien shrubs/fronds
- `WORLD/Contents/Vegetation/Grass/` — `GrassDefinition.cs`, light ground-cover families
- `SYSTEMS/Placement/` — object clearance and placement architecture
- `WORLD/Generation/Biomes/Definitions/` — biome-specific species choices and resources

The 2D game separates reusable species definitions from spawners and authored biome selections. Preserve that strength, but replace canvas artwork, 2D shadows, sprite sorting and 2D collision with their proper 3D equivalents.

## First implementation slice (later; not part of this README push)

1. Implement **one** reusable basalt boulder family with 2–4 model variants and a shared Inspector-editable definition.
2. Let the Test biome select its density/weights while using the existing chunk placement contract. Verify stable generation across unloaded/reloaded chunks, including negative coordinates and terrain slopes.
3. Validate visual ground contact, correct 3D collision, the protected spawn region and reasonable node counts.
4. Add one sparse alien plant/tree family only after rock placement is behaving well.
5. Introduce harvestability and a persisted removal overlay once the basic natural-feature world generation is stable.

**Out of scope for this document:** writing executable classes, moving the existing `ObjectsTest` placeholder scenes, modifying terrain meshes/shaders, or changing saves. This file is the architecture contract for future work.
