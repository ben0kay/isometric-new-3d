# ITEMS — Architecture & External Authoring Plan

> **Status:** Proposed for Fractured Horizons 3D; this Markdown is **not an implemented item system**.  
> **Based on:** `ben0kay/isometric-new-2d/ITEMS/`, especially `Definitions/README.md`, `Discovery/README.md`, `ItemDefinition.cs`, `ItemCatalog.cs`, and the connected loot/harvest systems.  
> **Project conventions:** Read root `PROJECT_STRUCTURE.md` and `NOTES/CodePreferences.md` first.

## 1. Goals

- **One canonical item definition per item**, authored as a Godot `.tres` Resource with a stable ID and explicit registry.
- Preserve the 2D **categorized, nested catalog** structure rather than accumulating item declarations in giant C# switch statements.
- Every umbrella folder follows the 3D project rule: root `ITEMS/` owns `ItemConfig.cs` + `ItemConfig.tres`. Detailed item-specific data lives inside its own named folder.
- Eventually support a Windows external editor (possibly an Avalonia application / BEN_TOOLBOX integration) to **browse, search, validate, add, modify and compare items**, recipe relationships, harvest drops and loot tables.
- External tooling is a **controlled authoring interface**, not an independent runtime item database. It must not bypass Godot resource identity, validation or catalog registration.
- Definition data is separate from runtime inventory stacks, dropped 3D objects, actual quantities, generated loot and player saves.

## 2. Proposed folder layout

Only make folders for actual content. The example item names illustrate the layout, not a request to create them all today.

```text
ITEMS/
├── README.md                               # This guide
├── ItemConfig.cs                           # Umbrella master settings (future)
├── ItemConfig.tres                         # Godot Inspector master config (future)
├── ItemDefinition.cs                       # Single resource type + stable item ID (future)
├── ItemCatalog.cs                          # Recursive explicit catalog and validation (future)
├── ItemCatalog.tres                        # Master item registry (future)
├── Definitions/
│   ├── RawMaterials/
│   │   ├── RawMaterialsCatalog.tres
│   │   ├── Ores/
│   │   │   ├── OresCatalog.tres
│   │   │   └── IronOre/
│   │   │       ├── IronOre.tres
│   │   │       └── IronOreIcon.png
│   │   ├── Minerals/
│   │   │   └── Rock/Rock.tres
│   │   └── Organics/
│   │       ├── OrganicsCatalog.tres
│   │       └── AlienResin/AlienResin.tres
│   ├── RefinedMaterials/
│   │   └── Ingots/
│   ├── Components/
│   │   ├── Electronics/CircuitBoard/CircuitBoard.tres
│   │   ├── Electrical/CopperWiring/CopperWiring.tres
│   │   └── Structural/StructuralBracket/StructuralBracket.tres
│   ├── Technology/
│   │   ├── Robotics/
│   │   └── AlienTech/
│   ├── Equipment/
│   │   ├── Tools/
│   │   ├── Weapons/
│   │   └── Backpacks/
│   ├── Food/
│   │   └── Raw/AlienBerry/
│   │       ├── AlienBerry.tres
│   │       └── AlienBerryConsumable.tres
│   ├── Consumables/
│   │   ├── Medical/
│   │   └── Fuel/
│   ├── Combat/
│   │   ├── Ammunition/
│   │   └── Explosives/
│   ├── Survival/
│   │   └── Camping/
│   ├── Placeables/
│   │   ├── Workstations/FieldWorkbench/
│   │   │   ├── FieldWorkbench.tres
│   │   │   ├── FieldWorkbenchPlaceable.tres
│   │   │   └── FieldWorkbenchRecipe.tres
│   │   ├── Storage/
│   │   ├── Structures/
│   │   ├── Lighting/
│   │   ├── Defenses/
│   │   └── Agriculture/
│   └── Vehicles/
│       ├── Parts/
│       └── Deployables/
├── World/
│   └── WorldPickup.cs                      # Future 3D dropped-item representation
└── Discovery/
    ├── ContentDiscovery.cs                 # Future read-only export/validation
    ├── ContentReportExport.tscn
    ├── ExportContentReport.ps1
    └── README.md                           # Later tooling instructions
```

**Folder path convention:** `ITEMS/Definitions/<Category>/<Subcategory>/<PascalItemName>/<PascalItemName>.tres`. Add deeper categories only when useful. Match related file names (`FieldWorkbench.tres`, `FieldWorkbenchRecipe.tres`). Store icons/models near their definitions if appropriate.

**One exception to scope:** `ITEMS/World/` is the shared dropped-item *representation*. Terrain props, placed buildings and creatures continue living in `WORLD/`, `SYSTEMS/Placement/`, or `ENTITIES/`; an item definition may reference the appropriate scene/capability instead of duplicating it.

## 3. Catalog hierarchy: registration is explicit

```text
ITEMS/ItemCatalog.tres
  → Definitions/RawMaterials/RawMaterialsCatalog.tres
    → Ores/OresCatalog.tres
      → IronOre/IronOre.tres (stable Id: iron_ore)
    → Organics/OrganicsCatalog.tres
      → AlienResin/AlienResin.tres (stable Id: alien_resin)
  → Definitions/Components/ComponentsCatalog.tres
    → Electronics/ElectronicsCatalog.tres
      → CircuitBoard/CircuitBoard.tres (stable Id: circuit_board)
```

- `ItemCatalog` loads nested `Categories` and direct `Items` once, rejecting circular references, duplicate IDs, invalid/missing definitions and broken resource dependencies.
- **Presence of a file does not mean registration**: tooling must link each new item into the intended subcategory catalog, creating/linking missing ancestor catalogs deliberately.
- Use stable `lower_snake_case` **item IDs** for saves and drop/recipe references; changing a file path or display name must not change its ID.
- Avoid a second hardcoded or JSON authoritative catalog. The Godot resources + explicit catalog tree are canonical.

## 4. Item definition versus runtime state

Proposed shared `ItemDefinition` fields:

| Group | Example fields |
| --- | --- |
| Identity | `Id`, `DisplayName`, `ShortName`, description, category/tags |
| Inventory | `MaxStack`, `WeightKg`, `VolumeLitres` |
| Appearance | `Icon`, tint and optional dropped-world presentation |
| Optional capabilities | `Consumable`, `Placeable`, `Equipment`, `Attack` references as needed |

A category specifies **what the object is**; capabilities specify **what it does**. A sleeping bag in `Survival/Camping/` could also reference a placeable capability.

Runtime stack records should hold a stable item ID, count and separately defined per-instance state only where needed (durability, charge, etc.). Never write current stack quantities back to shared `.tres` resources. Save formats identify items by stable ID, not transient Godot instance IDs.

## 5. Drops are separate from the items

Do **not** store a universal “drop chance” on `ItemDefinition`. The same item might drop at different rates from a rock, robot, alien tree or storage chest.

```text
SYSTEMS/
├── Harvesting/
│   ├── HarvestProfile.cs                   # Future: each world resource source's rewards
│   ├── HarvestDropEntry.cs                 # item ID, chance, min/max count
│   └── HarvestProfiles/                    # Optional shared profiles
├── Loot/
│   ├── LootTable.cs                        # Weighted chest/enemy loot
│   ├── LootEntry.cs
│   └── Definitions/                        # Optional per-source loot .tres
└── Crafting/
    ├── CraftingRecipe.cs
    ├── CraftingCatalog.cs / CraftingCatalog.tres
    └── Stations/                           # Recipes can live beside their items too

WORLD/Contents/...                           # Resource node owns harvest-profile reference
ENTITIES/Species/...                         # Enemy species owns loot-table reference
ITEMS/Definitions/.../<Item>/<Item>.tres     # Item data only
```

- A **harvest profile** describes independently rolled drops when a specific source is mined, cut, gathered or salvaged.
- A **loot table** describes weighted rewards, roll counts and amount ranges for enemies/containers.
- A **crafting recipe** consumes item IDs and yields item IDs; connect recipes to crafting stations without confusing the item definition with the station's runtime state.
- A rock placed in two biomes may be configured with a shared harvest profile or two intentional overrides; different biomes do **not** need duplicate item definitions.
- Random rolls must be reproducible/controlled when the system is deterministic; persist the *outcome* after a chest was generated/opened or a rock harvested so revisiting a chunk cannot farm duplicates.

**2D references:** `SYSTEMS/Harvesting/HarvestProfile.cs` and `HarvestDropEntry.cs`; `SYSTEMS/Loot/LootTable.cs` and `LootEntry.cs`; `SYSTEMS/Crafting/`.

## 6. Future external toolbox / item editor

The eventual authoring tool could be an Avalonia desktop app, BEN_TOOLBOX module or script-based workflow. Design the item system **now** to support that without forcing the first game pass to build a tool.

### Read/inspect workflow (first)

1. Use Godot .NET in **headless/editor-compatible export mode** to load project resources through `ResourceLoader` and validate them against their compiled C# classes.
2. Export `ContentIndex.json` (machine-readable) plus `ContentIndex.md` (human report) with item IDs, names, paths, categories, catalog membership, assets, crafting/harvest/loot relations and diagnostics.
3. The external tool reads the exported snapshot and can search/filter items, inspect references, spot duplicate IDs, missing icons, unused items, orphan recipes and sources with broken drops.
4. Every report carries source paths, export timestamp and validation status; **exports are snapshots, never a second authority**.
5. Consult the 2D `ITEMS/Discovery/README.md` for the existing proven concept and its `ExportContentReport.ps1` wrapper; adapt rather than blindly port.

### Controlled editing and creation (later)

- Start with a safe **new-item wizard**: choose category, assign unique stable ID, editable properties, icon and optional capabilities; write `.tres` in the correct folder and update exact catalog links.
- Extend to editing an existing item, recipe, harvest profile and loot table; show **where an item is used** before rename/delete.
- Prefer using Godot's resource APIs or a validated Godot-side import step for authoritative edits. Direct hand-editing `.tres` is possible but fragile (script refs, paths, encodings, IDs and format changes).
- A practical integration can exchange a **versioned JSON change request** with the app, then have a Godot editor/headless tool apply and validate it. JSON is transport, not runtime truth.
- Validate all referenced item IDs and range constraints; apply changes as an atomic/recoverable batch where practical, and provide diff/preview and backup or Git-friendly review before commit.
- Do not operate on a running game's active resources or user's save files. The editor changes *source definitions* in a development project.
- If a new/renamed item affects save compatibility, use explicit aliases/migrations; avoid silently breaking saved inventories.

### Potential editor screens

- **Item Library:** browse tree/catalog, search ID or name, edit stack/weight/tint and icon.
- **Drop Explorer:** choose a species, ore deposit or plant and inspect/edit harvest chances, weighted loot and min/max counts.
- **Recipe Links:** search an item and see recipes producing/consuming it, plus crafting station.
- **Validation Report:** missing catalogs, broken resources, duplicate IDs, unreachable content, save-ID migration warnings.

## 7. First implementation passes for Codex

- **I1 — Item foundation:** create `ItemConfig.cs/.tres`, `ItemDefinition.cs`, `ItemCatalog.cs/.tres`, explicit category registration and validation; one or two Test items (for example Rock and AlienResin).
- **I2 — Runtime storage and pickup:** implement tiny inventory + `WorldPickup` with 3D positioning, basic add/remove and save-friendly item IDs. Keep inventory ownership in `PLAYER/Inventory/` or a reusable storage domain rather than embedding stacks in definitions.
- **I3 — One real drop source:** Test biome rock or robot refers to a harvest/loot profile; collect an item and verify save/reload cannot respawn already-collected rewards.
- **I4 — Read-only discovery export:** port the 2D content report concept and export validated JSON/Markdown for a future external editor.
- **I5 — External authoring later:** browser/editor with resource-aware changes, validation, catalog updates, preview and Git review.

**Acceptance:** same ID resolves from master catalog; a new `.tres` without catalog registration is detected; duplicated IDs are rejected; one drop profile references a valid item; stack counts are saved independently of shared definitions; reports can find each item and its producing sources.

## 8. Relationship to other 3D documentation

- `PROJECT_STRUCTURE.md`: uppercase umbrellas, master configs, biome-generic content and procedural persistence.
- `NOTES/FeaturesToImplement.md`: AUDIO, FEEDBACK, COMBAT and UI Notifications; this item guide is the more detailed authority for future ITEMS design.
- `NOTES/CodePreferences.md`: method comments, regions, Inspector groups and preference for simple, performant reusable functions.

**Implementation note:** Only `ITEMS/README.md` is created in this pass. Future sample resources, catalogs, tools and working runtime systems are deliberately not claimed as present.
