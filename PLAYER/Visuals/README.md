# Explorer character prototype

A genuine skinned 3D recreation of the current 2D explorer's design: pale suit,
armour panels, graphite joints, blue visor, cyan displays and rear backpack.
This is a simplified prototype, not a finished likeness or production animation set.

## Files

- `Models/PlayerExplorer.glb`: ready-to-import model, seven materials, 17-joint
  skeleton and thirteen animation clips. About 4,700 vertices.
- `PlayerVisual.tscn`: model wrapper instantiated as `Visual` in `Player.tscn`.
- `PlayerAnimator.cs`: animation selection and transition blending.
- `Source/build_player.py`: reproducible editable geometry and animation source.
  Uses only Python's standard library. Run it from any working directory.
- `Source/import_in_blender.py`: imports the GLB into Blender, retaining its rig
  and actions, then saves `Source/PlayerExplorer.blend`. Run with Blender's
  `--background --python` option. Blender is not required to run the game.
- `Source/Reference/`: front/rear artwork used for this pass.
- `Models/PlayerExplorerPreview.png` and `PlayerExplorerWalk.gif`: offline visual
  checks rendered from the exported skinned asset, not screenshots of Godot.
- `Source/validate_player.py`: optional development validation and preview
  renderer; requires numpy and Pillow. Neither is a gameplay dependency.

## Animation behaviour

`Idle`, `Walk`, `Run`, `WalkBack`, `RunBack`, `WalkLeft`, `WalkRight`, `RunLeft`,
`RunRight`, `JumpStart`, `JumpAir`, `Fall`, `Land` are embedded in the model.
The animator sets loops at runtime and accepts imported library name prefixes.

Actual horizontal velocity drives gait and playback speed. Above walking speed,
the player switches to running. Facing remains controlled by the cursor.
Travel relative to facing selects forward, backward or side stepping.
Directional changes use an angular margin to avoid boundary chatter, and gait
transitions preserve normalized step phase. Playback speed is smoothed, and
idle/running thresholds have separate entry/exit values to avoid rapid toggling.
Diagonal travel currently picks its closest directional clip rather than blending
all directions in an AnimationTree; this is a deliberate prototype simplification.

All clips are in place. PlayerMotor still owns translation, gravity, jumps and
collision. The jump starts immediately on RMB, without delaying physics for a
wind-up animation. Ground transitions trigger the landing pose, including after
walking off a block. No root-motion tracks move the physics body.

Armour sections have rigid skin weights, suitable for a mechanical suit. Rounded
joint pieces disguise the articulation; this is not a continuous organic skin.
Foot positions and strides are approximate; ground IK and exact planted-foot
motion are future refinements. The imported bones remain editable in Blender.

## Tuning

`PLAYER/PlayerConfig.tres` exposes the defaults from `PlayerConfig.cs`:

| Setting | Purpose |
| --- | --- |
| ModelScale / ModelOffsetY | Fit the visual to the existing 1.8 m capsule |
| AnimationBlendSeconds | Transition duration between clips |
| AnimationMoveThreshold | Velocity below which idle plays |
| AnimationWalkReferenceSpeed | Metres/second represented by walk at playback 1 |
| AnimationRunReferenceSpeed | Metres/second represented by run at playback 1 |
| AnimationDirectionMarginDegrees | Extra angle before switching travel clips |
| AnimationSpeedSmoothness | How quickly cadence follows speed changes |
| WalkSpeed / SprintMultiplier | Actual gameplay movement speed |

Movement defaults and bindings remain arrows, Shift sprint and RMB jump.
At the existing 5 m/s walking speed the animation plays quite quickly; lower
WalkSpeed if a slower walking feel is desired. Change reference speeds to tune
cadence independently, at the cost of more visible sliding.

## Validation and local check

The exported GLB was checked for binary bounds, valid skin/joint indices, unit
weights, identity bind-pose skinning, normalized quaternions and loop continuity.
Its poses were rendered and visually reviewed using the offline QA script.
Godot/.NET and Blender were not available in the authoring environment, so the
Godot C# build, live import and Blender import script still require local checks.

Pull, let Godot import the GLB, build the C# project, then run the world. Check:

1. Idle shows the explorer instead of the old capsule.
2. Arrow movement animates limbs; Shift produces running.
3. RMB shows takeoff, airborne/fall and landing poses.
4. Moving opposite or sideways to the cursor uses directional gait variants.
5. Standing on the test blocks aligns the boots with the surface.

If the model is subsequently edited in Blender, export glTF Binary (`.glb`) over
the model path, including skinning and actions. Keep the clip names above. The
Python generator regenerates the prototype and will overwrite Blender changes
to the GLB, so do not run it over a hand-edited model unintentionally.
