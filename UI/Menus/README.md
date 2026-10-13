# Spatial menus — first 3D frontend pass

The 2D game's `BootScreen`, `MainMenu`, `PauseMenu` and `UIButtonFactory` were used as the visual/code references. **Godot Control UI remains in screen space** for legibility. `FrontEnd.tscn` contains a real perspective `Camera3D` traveling through a placeholder 3D command corridor; Main and Campaign panels fade as the camera moves. UI buttons are **not yet 3D-world interaction surfaces**. This is intentional.

## Flow

1. F5 opens `UI/Menus/Boot/BootScreen.tscn`; choose/create a profile.
2. `FrontEnd.tscn` displays the Main page, with title, profile card and cyan action buttons matching the 2D styling.
3. **Campaign** moves the camera forward to the Campaign viewpoint and reveals **Continue / New Campaign / Load Campaign / Back**.
4. **New Campaign** assigns a distinct world ID and seed. **Continue** opens the latest saved world for the selected profile. **Load** lets you select other saved worlds.
5. **Sandbox** enters the existing `WORLD/World.tscn` directly with save disabled.
6. Escape in the world opens the pause overlay: Resume, Save (campaign only), Options placeholder, Exit to Menu, Exit Game.

## Inspector and appearance

`UI/UiConfig.tres`: `CameraTravelSeconds`, `FadeSeconds`, `MainCameraPosition`, `CampaignCameraPosition`, `MenuWidth`, `MenuButtonHeight`.

`UI/Menus/FrontEnd/FrontEnd.cs` currently generates lightweight, temporary 3D corridor/planet props during startup. Replace with an authored ship/alien scene later without changing campaign/profile UI logic. The same scene owns both menu views and transitions.

## Test checklist (Godot .NET editor)

- F5 opens profile selection, new profile persists after restart, choose existing profile.
- Campaign click visibly travels through real 3D space; Back returns; rapid clicks do not queue overlapping tweens.
- Create two campaigns; Continue selects the most recent; Load lists both independently.
- In a campaign, walk several chunks, Escape → Save, Exit to Menu → Continue. Player returns near exact X/Y/Z in identical seeded terrain.
- Restart Godot and reload the same campaign. Alter `WORLD/WorldConfig.tres` in between to verify existing campaigns use their stored creation snapshot.
- Sandbox remains playable, but Save is disabled.
- Invalid/corrupt saved files do not silently overwrite a good backup; validation errors are surfaced. Test recovery with copies of disposable test saves.
- F6 on `WORLD/World.tscn` still works, with Escape pause and no profile requirement.

No Godot editor or .NET runtime was available to execute end-to-end tests during GitHub authoring; please run this checklist locally before treating the integration as verified.
