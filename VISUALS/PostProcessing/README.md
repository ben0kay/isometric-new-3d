# Contrast and lighting pass

VisualConfig.tres owns sun colour/energy, cool ambient fill and lightweight grading.
Sun azimuth/elevation and existing shadow bias remain unchanged by this pass.
Default resource tuning: warm-neutral sun energy 1.55; cool ambient energy 0.4.
Grade contrast 1.12, saturation 0.94, shadow lift 0.012 and vignette 0.04.
The contrast curve preserves its 0/1 endpoints; shadow lift restores a little visibility in the darkest ground.
One fullscreen canvas pass uses the existing Compatibility renderer. It does not require a renderer switch, SSAO, glow, HDR or fog.
WorldGrade canvas layer 10 is below DebugHud layer 20. Future UI should likewise draw above layer 10.
GradeEnabled = false skips the draw for comparison; it does not revert the sun/ambient changes.
For the earlier light balance, set SunEnergy 1.3, SunColor white, AmbientColor (0.65,0.73,0.85), AmbientEnergy 0.6.
Config changes are applied at startup and on Resource.Changed signals in the running process.
Editing the local resource in the editor is not guaranteed to synchronize into an already-running separate game: save and restart.
Native C# auto-properties do not automatically emit Resource.Changed for arbitrary assignments; code-driven tuning must call EmitChanged().
No terrain materials, mesh/collision, chunk generation, player, camera, fog or shadow bias settings are altered.
Screen sampling adds one fullscreen draw/screen copy. GPU performance and live appearance are not measured here.
Godot/.NET were unavailable during authoring: build, inspect shader errors, and compare the grade on/off locally.
If too dark, raise AmbientEnergy toward 0.5 or reduce GradeContrast toward 1.05.
If too washed out, lower GradeShadowLift. GradeVignette = 0 disables edge darkening.
