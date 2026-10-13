# Fractured Horizons — 3D Prototype

Side prototype to the 2D Godot/C# game. Separate architecture, shared gameplay vision.

## Prototype controls and rendering
- **Arrow keys**: move relative to the isometric camera.
- **Mouse cursor**: face the cursor independently of movement, including while idle.
- Flat test surface with collision, orthographic follow camera, and directional sunlight with shadows.
- The player uses a capsule placeholder. A small nose marker indicates its facing direction.

Open in **Godot 4.7 .NET** with .NET 8 SDK, build C#, then press F5.

## Master settings
- `PLAYER/PlayerConfig.tres`: movement speed, acceleration, cursor-facing, turn speed, **all currently implemented action key bindings**.
- `CAMERA/CameraConfig.tres`: camera angle, azimuth, size and smoothing.
- `WORLD/WorldConfig.tres`: test ground size.
- `VISUALS/VisualConfig.tres`: sun angle, shadow enable/quality, ambient strength.
- `CORE/CoreConfig.cs` and `DEBUG/DebugConfig.cs`: reserved for future settings.

Key bindings are created from PlayerConfig at game startup. This avoids duplicate keys stored in project.godot.
Directional shadow-map resolution is raised to 4096, and sunlight shadow coverage is limited to improve close-up detail. If the shadow still shows artifacts, try ShadowBias and ShadowNormalBias in VisualConfig and check the selected rendering backend.

See `NOTES/ARCHITECTURE.md` for architecture decisions. This commit is not runtime-tested in Godot.
