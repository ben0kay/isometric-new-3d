# 3D Project Architecture — Pass 1 / Cursor Facing

Godot 4.7 .NET / C#. The 2D project is design reference, not code to port verbatim.

## Master config convention
Each major umbrella owns a Resource script and a .tres asset when in use.
The .tres is the single saved source of tuning values; scripts provide defaults.
Core and Debug config classes currently have no tunable fields.

## Ownership
- PLAYER: Player.cs coordinates movement and visual facing; Movement/PlayerInput.cs owns action names and binds the keys from PlayerConfig; Movement/PlayerMotor.cs owns collision-aware movement.
- CAMERA: world-owned independent Camera3D and CameraConfig.tres.
- WORLD: composition scene and disposable flat terrain.
- VISUALS: sunlight, ambient and shadow settings, independent of movement.
- CORE/DEBUG: expansion points, not speculative implementations.

## Design notes
- Arrow keys default in PlayerConfig; runtime InputMap bindings come from the config at startup. Don't also hardcode keys in project.godot.
- Cursor facing uses a camera ray intersected with a horizontal plane through the player. This provides reliable 360-degree aiming on the flat prototype, including stationary turning.
- Camera-relative movement remains separate from aim facing.
- Shadow quality depends on renderer, shadow-map resolution, coverage distance and bias. Shadow settings are in VisualConfig.
- Resource changes take effect upon next launch; live editing can be added later.

## Future scope
Terrain chunks, AI, combat, persistence, animations and inventory are intentionally deferred.
