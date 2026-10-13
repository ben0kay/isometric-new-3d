# Fractured Horizons — 3D Prototype

Side prototype to the 2D Godot/C# game. Separate architecture, shared gameplay vision.

## Prototype controls
- **Arrow keys**: move relative to the isometric camera.
- **Shift** (hold): sprint; multiplier configurable in PlayerConfig.
- **Right mouse button**: jump while grounded; jump velocity configurable in PlayerConfig.
- **Mouse cursor**: face the cursor independently of movement, including while idle.

Open in **Godot 4.7 .NET** with the .NET 8 SDK, build C#, then press F5.

## Master configs
- `PLAYER/PlayerConfig.tres`: all implemented control bindings, movement speed, sprint, jump, cursor facing.
- `CAMERA/CameraConfig.tres`: camera angle, azimuth, size and smoothing.
- `WORLD/WorldConfig.tres`: test ground size.
- `VISUALS/VisualConfig.tres`: sunlight, shadow enable/quality, ambient light.

Inputs are bound at startup from the PlayerConfig resource, rather than duplicated inside project.godot.
The 1920×1080 project window settings from the subsequent local push are preserved.
The Godot-generated C# UID files from that push are also retained.
See `NOTES/ARCHITECTURE.md` for responsibilities. Runtime build/test is still pending.
