# Campaign persistence — version 1

Adapted from the 2D game's `SYSTEMS/Profiles/ProfileStore.cs`, `SYSTEMS/Saving/CampaignData.cs`, `CampaignStore.cs`, `CampaignSession.cs`.

- Profiles: `user://Profiles/profiles.json` with stable GUID IDs, backup, and an in-memory selected profile.
- Campaigns: `user://Profiles/<profile GUID>/Campaigns/<campaign GUID>.json`. **Multiple campaigns per profile**, individual names, timestamps and seeds.
- Coverage: `world-player-only` version 1, saving `WorldGenerationSettings` (creation seed, finite dimensions, terrain generation settings) and player X/Y/Z. Shared `.tres`, nodes, meshes and collision are never serialized.
- Save storage uses validated JSON, a flushed temporary file, and replacement with a backup. Existing corrupt primaries are quarantined rather than silently destroying data.
- Loading instantiates a **detached** `WORLD/World.tscn`, invokes `ChunkManager.UseSavedSettings` **before** it enters the scene tree, then assigns saved player position. ChunkManager's existing synchronous collision ring runs in `_Ready` before physics.
- Campaign save is manual via Escape → Save; new campaigns write an initial slot. Sandbox has no campaign and cannot save.
- The system is a starting schema. Do **not** add empty entity, inventory, crafting or constructed-object sections before their systems exist. Future version changes must use explicit migrations.
- World-dependent data must eventually include a persistent overlay for harvested objects and built structures keyed by stable world/chunk/object IDs.

**Known limitation:** global scene objects and input are only validated by local Godot execution; the GitHub authoring environment cannot compile/run the engine. Run `UI/Menus/README.md` test checklist.
