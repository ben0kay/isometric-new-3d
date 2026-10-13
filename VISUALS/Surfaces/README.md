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

## Detail pass adapted from 2D
The shader now uses four rotated noise octaves and domain-warped masks to make patch edges less blob-like.
Generic recipes provide grass fibres, soil grain, muted sediment/gravel flecks, and rock fractures.
Test palette is muted; patch scale is 10 m, medium detail scale 0.35 m, and detail strength 0.35.
Fine detail fades with screen footprint to reduce shimmer. No physical mesh displacement or baked lighting is added.
C# surface profile and material factory use regions according to NOTES/CodePreferences.md.
This remains procedural appearance, not an exact reproduction of the 2D baking/lighting pipeline.
Extra noise evaluations increase shader cost; GPU performance has not been measured here.

## Diagonal line diagnosis
VISUALS/VisualConfig.tres owns the active sun settings; SunLighting applies them on world startup.
1. Uncheck SunShadows and restart gameplay. If the lines disappear, shadow self-interference is confirmed.
2. Re-enable SunShadows. Try ShadowNormalBias 1.5, then 2.0 if needed, restarting each time.
3. If necessary try ShadowBias 0.15, then 0.2. Stop increasing if shadows visibly detach or become too thin.
Current code defaults: normal bias 1.0, bias 0.1, max shadow distance 45 m.
These are diagnostic starting values, not verified settings for every camera/renderer.
If lines remain with shadows disabled, inspect shader/detail or other rendering artifacts instead.
Lighting was deliberately not changed by this surface-detail pass.

## Cohesive ground pass
Reviewed the 2D GroundSun shader, generic grass/dust/mud/basalt/grey/steep rock recipes,
BiomeGroundMap's padded biome/slope maps, and GroundFog's separate animated overlay.
The relevant lesson is dominant mineral ground, subordinate surface masks and restrained scale-separated detail.
This pass does not copy 2D biome-map baking, painted lighting, heavy Voronoi rock loops, or fog into the 3D shader.

### Current composition
- Mineral soil is the continuous base; normalized secondary weights leave at least 35% base before slope rock.
- Coverage locates regions; GrassBlendStrength/DustBlendStrength limit how strongly they alter the base.
- Test's secondary strengths total at most 0.62, so its base retains at least 38% on gentle ground.
- Broad 42 m masks are rotated/stretched and lightly warped for fixed directional ground flow.
- Fine 0.22 m detail and filtered grain define material identity; medium-scale high-contrast colour mottling is removed.
- A muted charcoal/olive/earth palette keeps secondary materials related.
- Grass fades toward base soil between 12–28 degrees; rock starts at 30 and reaches full strength at 48 degrees.
- Slope breakup is small and affects masks, not coordinate rotation, preventing contour-following streaks.
- Two broad noise octaves replace the former four-octave field for each competing patch.
- One shared material, no per-frame updates, new texture allocations or per-chunk duplicates.
- Visual patterns remain tied to absolute world coordinates and saved seed; no TIME or camera-space placement.
- Heights, mesh, collision, streaming, player, camera, light and fog code are unchanged.

### Inspector tuning
Edit WORLD/Generation/Biomes/Definitions/Test/Surfaces/TestSurfaces.tres and restart gameplay:
| Setting | Effect |
| --- | --- |
| SoilLow / SoilHigh | Dominant mineral-earth palette |
| GrassCoverage / DustCoverage | Region distribution; not exact area percentages |
| GrassBlendStrength / DustBlendStrength | Secondary material influence; lower for more base |
| PatchScaleMeters | Larger = broader regional change |
| BlendSoftness | Larger = gentler transitions |
| FlowAngleDegrees / FlowStretch | Direction and elongation; stretch 1 removes elongation |
| WarpStrength | Gentle bending of the broad regions |
| MacroVariation | Broad brightness modulation, independent of grain |
| DetailScaleMeters / DetailStrength | Fine material size and intensity |
| SoilStartDegrees / SoilFullDegrees | Fade vegetation toward soil on intermediate slopes |
| RockStartDegrees / RockFullDegrees | Exposed rock transition on steep ground |
| Material colour pairs | Biome palette remains fully editable |

Test seam continuity, repeat visits, seed restart, disabled-secondary base, and gentle/intermediate/steep slopes.
Static checks cover uniform bindings, weight sums/bounds and shader structure. Live Godot compilation,
appearance and GPU timings remain unverified in this environment. Fewer noise calls do not establish a measured FPS improvement.
The 2D reference also has darker lighting, long shadows, detailed props and mist; those differences remain outside this pass.
