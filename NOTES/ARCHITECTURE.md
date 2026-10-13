# 3D Architecture — Prototype

Godot 4.7 .NET / C#. Concepts from the 2D project are references, not files to port verbatim.

## System ownership
- PLAYER/PlayerConfig.tres is the source of truth for all current player action keys and mouse buttons, plus walk/sprint/jump/facing tuning.
- PLAYER/Movement/PlayerInput.cs converts configured bindings into Godot InputMap actions.
- PLAYER/Movement/PlayerMotor.cs owns camera-relative movement, grounded jumping and sprint speed.
- PLAYER/Player.cs coordinates systems and independently rotates the visible character toward the cursor via camera-to-plane ray intersection.
- CAMERA owns camera tracking, projection and camera configuration.
- WORLD owns test scene assembly and disposable test ground.
- VISUALS owns sun, shadows and ambient settings.
- CORE and DEBUG master config classes exist as future expansion points.

## Rules
Keep editable configuration separate from runtime state.
Each value belongs to one config and isn't hardcoded in other systems.
Add folders as actual features arrive rather than empty speculative architecture.
No procedural chunks, saving or combat yet.

## Controls
Arrow keys move, Shift sprints, right mouse button jumps, cursor controls heading.
All controls configurable in PlayerConfig.cs/tres, applied on _Ready.
