# 3D Project Architecture — Pass 1

Godot 4.7 .NET / C#. The 2D project is design reference, not code to port verbatim.

## Umbrella master config convention
Each major umbrella owns a config Resource script and a .tres asset (CORE, PLAYER, CAMERA, WORLD, VISUALS, DEBUG).
The .tres resource is the one saved source of tuning values; the scripts provide defaults.
Keep a value owned by a single config. Avoid duplicate override values in scene nodes or unrelated scripts.
Empty configurations are intentional expansion points; do not invent unrelated systems in Pass 1.
Resource edits are picked up on the next scene launch; live-runtime editing may be added later.

## Ownership
- CORE: cross-cutting configuration only.
- PLAYER: Player.cs coordinates; Movement/PlayerInput.cs reads input; Movement/PlayerMotor.cs owns physical motion. Visual is replaceable.
- CAMERA: independent Camera3D with target reference and projection/follow settings.
- WORLD: composition scene and disposable test terrain, later world streaming.
- VISUALS: sun and environment settings, never player movement.
- DEBUG: later debug tooling.
- NOTES: architecture and implementation decisions.

## Scene wiring
World.tscn is the main scene. Player (CharacterBody3D), IsometricCamera (Camera3D), Sun (DirectionalLight3D) and TestGround are independent instances.
Player has a reference to camera for camera-relative movement; camera has a target to follow. Replace this minimal bridge with a dedicated direction provider if camera modes expand.

## Pass 1 scope
Flat surface, WASD movement, capsule placeholder, orthographic elevated follow camera, directional sun + shadows.
Not yet: procedural chunks, combat, save games, animation, equipment, inventory, or creatures.

## Tuning
PLAYER/PlayerConfig.tres — walking speed, acceleration, drag/gravity, facing.
CAMERA/CameraConfig.tres — elevation, azimuth, orthographic size, following.
WORLD/WorldConfig.tres — size of test floor.
VISUALS/VisualConfig.tres — sun elevation/azimuth, energy, shadows, ambient energy.

## Future rules
Prefer definitions/resources for editable data; use nodes for runtime state and scene ownership.
New subsystems earn folders when implemented; avoid empty boilerplate architecture.
