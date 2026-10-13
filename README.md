# Fractured Horizons — 3D Prototype

Side prototype to the 2D Godot/C# game. Separate architecture, shared gameplay vision.

## Pass 1
- Flat test surface with physical collision
- WASD camera-relative walking and simple placeholder capsule
- Independent orthographic isometric camera following player
- Sun lighting and shadow casting
- Per-umbrella master .tres configuration resources

Open the project in **Godot 4.7 .NET** (with .NET 8 SDK available), let C# compile, and press F5.

Edit configs in `PLAYER/PlayerConfig.tres`, `CAMERA/CameraConfig.tres`,
`WORLD/WorldConfig.tres`, `VISUALS/VisualConfig.tres`.
See `NOTES/ARCHITECTURE.md`.

The Godot scene is a first-pass mockup and has not been runtime-tested in this environment.
