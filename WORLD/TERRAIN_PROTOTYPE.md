# Finite Test terrain prototype

## Implemented
- One active biome: `WORLD/Generation/Biomes/Definitions/Test/Test.tres`.
- 32 × 32 chunks, each 64 m: **2048 × 2048 m**, centered around origin.
- 32 cells per chunk (2 m spacing), gentle deterministic hills and a flat spawn clearing.
- Absolute-coordinate value noise with generator version 1. Identical edge samples and central-difference normals.
- Indexed terrain mesh; collision is generated from the same mesh.
- Shared streaming: nearest missing chunks load first, one complete chunk per rendered frame by default.
- A synchronous 3 × 3 collision safety ring is maintained before player physics. Initial spawn waits for this ring.
- Radius 2 loads a 5 × 5 square; radius 3 retirement hysteresis avoids repeated seam loading.
- Four invisible perimeter colliders prevent walking off the finite world. Boundary visuals remain a later pass.
- Old TestGround files remain available but are no longer instantiated by World.tscn.
- No player animation, input, camera or lighting changes.

## Tuning
Edit `WORLD/WorldConfig.tres`: seed, dimensions **in whole chunks**, chunk size, grid resolution, streaming radii and build budget.
Edit `TestTerrain.tres`: base elevation, hill amplitude and feature scale.
Terrain uses WorldConfig's existing ground colour. Grid/test blocks remain specific to the legacy TestGround scene.
The prototype caps dimensions at 128 chunks and chunk size at 128 m; it is not the proposed 512 km world.
Generation is synchronous and the frame budget is a chunk count, not a guaranteed millisecond limit. Safety prefetch can exceed the normal budget. Profile before increasing resolution or radius.

## Save contract
`WorldGenerationSettings` is a JSON-serializable creation snapshot: generator version, seed, biome ID, finite dimensions, chunk size/resolution and all current elevation parameters.
Future saves must capture it with persistent world/profile IDs and player logical coordinates.
Call `ChunkManager.UseSavedSettings(snapshot)` **before adding the world scene to the tree** to restore the baseline; position the player before the scene enters the tree too.
The current scene creates a new snapshot from resources each launch. **There is no disk save, boot menu, profile UI or save/continue yet.**
Changing a .tres affects new worlds; future continued worlds must use their saved snapshot. Unknown generator versions are rejected rather than silently changing terrain.
No generated meshes or runtime nodes belong in saves. Persistent harvested/built/modified objects will use a separate changes overlay keyed by stable chunk/object IDs.
The API supports settings restore only; complete player restore, including velocity and persistent edits, is a later saving pass.
At prototype distances logical coordinates currently equal scene coordinates. Floating origin is required before long-range worlds.

## Validation
Build C# in Godot, then run:
```sh
godot --headless --path . WORLD/Tests/TerrainPrototypeChecks.tscn
```
Use the Godot .NET executable, replacing `godot` with its local executable name/path.
Checks exercise negative coordinates, finite bounds, actual mesh seam heights/normals, collision face count, JSON snapshot restoration and generator-version rejection.
Then play World.tscn:
1. Walk/run/jump across hills and chunk edges; camera follows elevation.
2. Travel several chunks and return; geometry remains the same.
3. Inspect the remote Terrain node; old chunks retire.
4. Temporarily set dimensions to 2 × 2 chunks to reach all four blocking edges quickly.
5. Restart with the same seed; baseline terrain reproduces.
6. Keep F8 animation diagnosis available if movement judder returns.

Godot/.NET were unavailable in the authoring environment: live mesh rendering, C# compilation, Jolt collision seams and streaming performance require these local checks.
