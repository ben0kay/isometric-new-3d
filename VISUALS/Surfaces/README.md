# Basalt-first ground: faithful 2D material adaptation

This implementation supersedes earlier soil/grass camouflage experiments.
Source: ben0kay/isometric-new-2d, especially GroundSun.gdshader and VISUALS/Drawings/Ground/.

## What was ported
| Reference | 3D adaptation |
| --- | --- |
| GroundSurfaceNoise.cs | Same 512 × 512 seamless normalized SimplexSmooth texture, seed 64127, frequency 0.025, four FBM octaves; shared and mipmapped |
| BasaltSurface | Original colour mix, sparse fracture gate, detail and grain arithmetic; palette/strength become parameters |
| DustSurface | Original muted sediment formula and grain amplitudes |
| MineralSurface | Original narrow fracture veins and restrained reflected mineral tint; no emission |
| GreyRockSurface | Original exposed-grey-rock mix, fractures and grain |
| SteepRockSurface | Original two neighborhood searches for irregular angular joints/chips; palette parameterized |
| GrassSurface | Original optional neutral fibre/mottle recipe |
| MudSurface | Original warped winding-channel recipe |
| StonyPathSurface | Original subdued gravel/fleck recipe |
| GroundSun layering | Basalt → dust → minerals → optional weathering/grass/mud/stony layers → slope rock |
| BiomeGroundMap | Preserve separation of biome data from presentation; use existing 3D mesh normals instead of padded 2D biome/slope texture baking |

Sampling frequencies/offsets at defaults match GroundSun exactly:
dust 0.0039, minerals 0.0061, strata 0.035, detail 0.22, grain 2.7.
MetersPerReferenceTile maps actual meters to logical reference tiles. Default is one meter per tile.
OffsetPatternByWorldSeed defaults false to match the reference's fixed noise seed/phase.
Enabling it translates the same shared texture by a reproducible saved-world-seed offset.
Reloading chunks does not create random samples or reset local UVs.

The previous shader used custom hash/value noise, altered masks, a mandatory soil base and competing normalized grass/dust weights.
Those no longer drive the main ground shader. Old helpers retained on disk are unreferenced legacy utilities.

## Test defaults
Exact reference basalt/dust/mineral numeric palette and composition defaults: dust 0.55, minerals 0.32.
No grass, soil replacement, mud, or stony overlay by default.
Grey exposed rock starts at 22° and reaches full strength at 38°; angular blocks are enabled.
No new fog, lights, cast-shadow tuning, terrain geometry, collision, streaming, player or camera behavior.
Colour coefficients are bound directly (without an extra source_color conversion) to preserve original formula values.
They are configurable in the C# profile Inspector; changes are applied on game startup.

## Inspector controls
Edit WORLD/Generation/Biomes/Definitions/Test/Surfaces/TestSurfaces.tres, rebuild C# first, then restart gameplay.
| Setting | Effect |
| --- | --- |
| BasaltLow / BasaltHigh | Dominant charcoal volcanic base |
| DustCoverage / DustBlendStrength | Deposit distribution / influence |
| MineralAmount / MineralLow / MineralHigh | Sparse reflected mineral seams |
| RockStartDegrees / RockFullDegrees | Smooth exposed-rock slope transition |
| SteepRockBlocks | Original angular joints versus cheaper grey-rock recipe |
| Rock/SteepRock colour pairs | Weathered stone and mountain-face palette |
| WeatheredRockAmount | Optional grey-rock patches on flatter ground (default 0) |
| MetersPerReferenceTile | World scale of all reference details; larger stretches materials |
| DepositScale | Broad dust/mineral scale only; larger broadens deposits |
| BlendSoftness | Original mask widths at 1; lower sharpens, higher softens |
| GrainStrength / FractureStrength | Fine grain and joint intensity; reference values at 1 |
| DetailStrength / ReliefStrengthMeters | Subtle engine-lit microrelief; set relief to 0 to compare flat albedo |
| SurfaceBrightness | Material brightness without altering scene lighting; reference coefficients at 1 |
| Optional grass/soil/mud/stony controls | Future biome flexibility; Test stays barren |

## Lighting/relief differences from 2D
Original canvas shader multiplies colour by custom world sunlight and a sunward sampled brightness relief.
The spatial shader uses real mesh normals and Godot's lights, shadows and ambient lighting.
Reference relief's dust/grass attenuation is retained, but a small derivative normal perturbation replaces baked directional brightness.
This does not displace vertices or modify collision; it can be disabled with ReliefStrengthMeters = 0.
The 2D fog overlay, path geometry mask, surface resource deposits, richer rock sprites and custom shadow pipeline are not ported here.
The screenshot therefore cannot be expected to match pixel for pixel even with identical material formula coefficients.

## Performance and limitations
One shared 512 × 512 noise texture and one material for all Test chunks; no per-chunk duplicates.
NoiseTexture2D generates asynchronously once, so initial frames may precede its completed image.
Mipmap filtering reduces distant grain shimmer. No full-world colour bake or per-frame CPU sampling.
Flat Test ground takes five shared noise texture reads. Optional layers add samples only when enabled.
Steep fragments run the original 18-cell fracture searches; turn off SteepRockBlocks for a cheaper mountain material.
This is a direct reference port, not a measured performance improvement. GPU timing remains unmeasured.
The reference noise texture repeats; original differing sample frequencies/offsets reduce obvious coordinated repetition.
Very large coordinates still require the project's future logical-coordinate/floating-origin solution.

## Verification
Static checks verify formula parity (default parameters), exact reference texture settings, uniform/resource binding,
disabled grass defaults, signed absolute-coordinate continuity, and no geometry/simulation files changed.
Godot/.NET are unavailable here: C# build, shader compilation, rendered comparison and GPU timings are NOT claimed.
After pulling:
1. Build C#, restart World.tscn; inspect shader errors if any.
2. Wait for the shared noise image to finish, then compare flat ground to the old screenshot.
3. Cross positive/negative seams and travel away/back; the same location must retain its pattern.
4. Climb toward X -100/Z -100; inspect gradual exposed-rock transition and angular joints.
5. Compare SteepRockBlocks on/off and ReliefStrengthMeters 0/0.035.
6. Inspect colours with unchanged scene lighting. If too dark, adjust SurfaceBrightness modestly (e.g. 1.1–1.3).
7. Confirm Test.tres still references TestSurfaces.tres after resource editing.
