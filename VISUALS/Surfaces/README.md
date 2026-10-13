# Ground surface prototype

One shared spatial shader blends soil, grass, dusty gravel and slope rock on existing terrain.
Generic .gdshaderinc files provide reusable palette functions; shared helpers provide world-space noise/masks.
Test's settings live in WORLD/Generation/Biomes/Definitions/Test/Surfaces/TestSurfaces.tres and are referenced by Test.tres.
GroundSurfaceMaterial.cs applies that resource once at world startup. Restart gameplay after editing the profile.
GrassCoverage/DustCoverage are mask controls, not exact area percentages; dust blends over grass, then slope rock blends over both.
Rock fades in from 18 degrees to 36 degrees by default, with subtle noise breakup.
PatchScaleMeters sets broad patches; DetailScaleMeters/DetailStrength set fine grain, faded with distance to reduce aliasing.
No TIME, per-chunk RNG, vertex displacement, geometry/collision changes or duplicate chunk materials.
World X/Z and saved terrain seed drive cosmetic patterns; there is no camera-dependent patch placement.
Purely visual palettes currently follow resources on restart, not a saved surface snapshot. Save versioning for authored biome appearance remains future work.
This is procedural colour/grain, not photographed textures, physical gravel, grass meshes, or a full triplanar texture system.

Local validation:
1. Build C#; launch World.tscn and check Godot's shader error panel.
2. Walk across positive/negative chunk seams; inspect colour continuity.
3. Travel away/back; patterns at the same position should remain fixed.
4. Climb the mountain at X -100/Z -100; look for gradual grey rock on its steep flanks.
5. Set coverages to 0/1 and adjust palette/patch size; restart and check expected changes.
6. Compare collision/movement to before; shader only changes ALBEDO/ROUGHNESS/SPECULAR.
Godot/.NET were unavailable during authoring. Shader compilation, live appearance and GPU timings require local verification.
