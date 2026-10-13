# Features to Implement — 2D → 3D

> **Project:** Fractured Horizons, 3D isometric prototype.  
> **Status:** Planning backlog, **not implemented** unless explicitly identified.  
> **Source:** `ben0kay/isometric-new-2d` on GitHub, inspected 2026-10-12.  
> **Read first:** Root `PROJECT_STRUCTURE.md`, `NOTES/CodePreferences.md`, and current live source before implementing.

This is a **selective translation guide**, not a command to port all 2D code. The aim is to keep the mechanics and the useful data-driven architecture while implementing proper 3D actors, `Node3D`/Control UI, `AudioStreamPlayer3D`, true terrain height, and the new saving/streaming rules.

## Project-wide rules for new features

- **New top-level gameplay umbrellas are ALL_CAPS** and have a master `<Umbrella>Config.cs` plus `<Umbrella>Config.tres` for Godot Inspector tuning. Example: `FEEDBACK/FeedbackConfig.cs` and `FEEDBACK/FeedbackConfig.tres`. Domain-specific settings/resources can still live beneath them.
- Uppercase applies to umbrellas, not to all nested folders: `FEEDBACK/CameraShake/` and `AUDIO/Sounds/Player/`.
- **One owner per responsibility.** Gameplay determines what happened; feedback, audio and UI present it. Don't make combat code instantiate floating labels, and don't let UI become the save system.
- Compose small, independently testable parts. Add only the folders/classes required for the current implementation pass; example trees below are **targets**, not blank-file tasks.
- All world effects must account for **real X/Y/Z height**, isometric camera projection and streamed entity lifetimes. Do not reuse 2D `Node2D` positioning assumptions.
- Resources provide defaults; per-enemy/per-action overrides are optional. Feature settings should not be scattered across other umbrellas.
- Distinguish **saved game state** from transient effects: a damage number, audio player or camera-shake tween does not belong in a save.
- Unless requested separately, this Markdown task does **not** create code, `.tres`, scenes, or registered autoloads.

---

## Feature 1 — FEEDBACK umbrella

**2D origin:** `VISUALS/Feedback/` (note: **not** a root `FEEDBACK/` folder in the 2D repo). Existing 2D files include `WorldFeedback.cs`, `HealthBars/`, `DamageNumbers/` and `CameraShake/`. For 3D, **promote the cohesive presentation system to its own root `FEEDBACK/` umbrella**, consistent with the desired uppercase structure.

### Proposed 3D layout

```text
FEEDBACK/
├── FeedbackConfig.cs / FeedbackConfig.tres  # Umbrella master toggles, limits and defaults
├── FeedbackManager.cs                       # Narrow coordinator, not gameplay authority
├── HealthBars/
│   ├── HealthBar3D.cs                       # 3D anchor -> camera-facing bar
│   ├── HealthBarSettings.cs
│   └── DefaultHealthBarSettings.tres
├── DamageNumbers/
│   ├── DamageNumbers3D.cs                   # Damage amounts projected/anchored to actors
│   ├── DamageNumberSettings.cs
│   └── DefaultDamageNumberSettings.tres
└── CameraShake/
    ├── CameraShakeBus.cs                    # Event requests, independent of camera scene
    ├── CameraShakeController.cs             # Applies bounded shake to active camera
    ├── CameraShakeSettings.cs
    └── DefaultCameraShakeSettings.tres
```

**Useful 2D behaviour worth retaining:**
- Health bars appear when hurt and can have creature-specific settings; scale moderately for high max-health values; disappear when the owner is retired/dies.
- Damage numbers report **actual damage applied**, can merge closely spaced hits, and have bounded display lifetimes, settings for fade/pop/offset and optional per-species overrides.
- Camera shake is centrally requested; strength, duration and falloff should be adjustable and stack/cap safely rather than letting hits permanently displace the camera.
- World-space presentation should track 3D anchors yet remain legible to an orthographic/isometric camera. Choose carefully between projected `Control` overlays and `Label3D`/billboards. Use pooling/visibility budgets where profiling justifies them.

**Interfaces:** future `COMBAT/Health` emits signals/events; `FEEDBACK` observes and displays. `CAMERA/` remains owner of camera movement; shake applies a **temporary visual offset** without corrupting the follow-camera position. `UI/Notifications/` handles alerts/toasts, not actor feedback.

**Start small:** one configurable 3D health bar and one test damage number above a test actor; then implement camera shake.

**References:** `VISUALS/Feedback/README.md`, `VISUALS/Feedback/HealthBars/`, `VISUALS/Feedback/DamageNumbers/`, `VISUALS/Feedback/CameraShake/`.

---

## Feature 2 — AUDIO umbrella

**2D origin:** `AUDIO/` already is a proper root umbrella. It includes `AudioManager.cs/.tscn`, `AudioSettings.cs/.tres`, `AudioCatalog.cs/.tres`, `AudioDefinition.cs`, `AudioBuses.tres`, `Sounds/`, `Music/`, `Ambience/` and `Companion/`.

### Proposed 3D layout

```text
AUDIO/
├── AudioConfig.cs / AudioConfig.tres         # 3D master config (name matches umbrella rule)
├── AudioManager.cs / AudioManager.tscn       # Central playback, pooling, emitter lifetime
├── AudioDefinition.cs                       # Per-sound data (mode, source, spatial, limits)
├── AudioCatalog.cs / AudioCatalog.tres      # Stable sound IDs and definitions
├── AudioBuses.tres                          # Native Godot mixer buses
├── Sounds/
│   ├── Player/                              # Footsteps, jump, landing, equipment
│   ├── Combat/                              # Shots, hits, shields, explosions
│   ├── Environment/                         # Generators, machines, wind, wildlife
│   ├── Vehicles/                            # Buggy engine, wheels, impacts (later)
│   └── UI/                                  # Button presses, alerts
├── Music/                                   # Menu/exploration/danger themes
├── Ambience/                                # Per-biome and interior beds/emitters
└── Companion/
    └── CompanionVoice.cs                   # Future warnings/voice sequencing
```

**Useful 2D behaviour worth retaining:**
- One-shot, looping and intermittent sound definitions, with per-ID cooldown and maximum instances.
- Central category budgets, pooling, priority/preemption, fade-in/out and owner-managed loop registrations.
- Distinct buses for Master, World/SFX, Music, Ambience, UI and Companion/voice; allow volume adjustments from Options.
- Spatial vs non-spatial is **separate from** one-shot/loop mode. Use `AudioStreamPlayer3D` and the appropriate listener/attenuation for world sounds; `AudioStreamPlayer` for music, UI and non-spatial messages. Don't port 2D stereo/panning math.
- Emitters should stop or unregister when a streamed actor/chunk unloads; avoid per-chunk permanent audio-player arrays.
- Biomes can reference ambience definitions, but the central `AUDIO/` umbrella owns the sound system and reusable clips. Later interior/cave zones can apply indoor/outdoor ambience and reverb transitions.
- A future companion voice warning may duck music/ambience temporarily.

**First prototype:** an adjustable player footstep, an outdoor ambient loop, a UI chime and a positional machine hum. Verify correct attenuation in 3D, buses, owner cleanup and no looping duplication.

**References:** `AUDIO/README.md`, `AUDIO/AudioManager.cs`, `AUDIO/AudioSettings.tres`, `AUDIO/AudioCatalog.tres`, `AUDIO/Sounds/`, `AUDIO/Ambience/`.

**Naming decision:** 2D calls the primary resource `AudioSettings`; 3D should expose **`AudioConfig`** as the umbrella master. Keep specialized definitions/settings internally without creating a competing second global config.

---

## Feature 3 — COMBAT umbrella

**2D origin:** `COMBAT/` includes `Health.cs`, `DamageTypes.cs`, `DefenseDefinition.cs`, `Attacks/`, `Projectiles/`, `Hitboxes/`, `Cover/` and `Weapons/`. The 2D combat planning file describes additional **proposed**, not guaranteed implemented, mechanics.

### Proposed 3D layout

```text
COMBAT/
├── CombatConfig.cs / CombatConfig.tres
├── Health.cs                            # Shared health/hull authority
├── DamageTypes.cs
├── DefenseDefinition.cs
├── Attacks/
│   ├── AttackDefinition.cs
│   ├── ContactAttack.cs
│   └── ProjectileAttack.cs
├── Projectiles/
│   ├── Projectile.cs / Projectile.tscn
│   └── ProjectilePool.cs
├── Hitboxes/
│   └── CombatHitbox3D.cs
├── Cover/
│   └── CombatCover3D.cs
└── Weapons/
    └── Weapon.cs
```

**3D translation:** real 3D hit volumes/rays, projectile trajectories, height differences, terrain and building obstruction, slope-aware explosions, configurable damage channels and pooled effects. Combat data remains independent of `FEEDBACK/` and `AUDIO/`; it requests events, and these umbrellas present the response. Do not create a giant combat system in the player controller.

**First prototype:** one target dummy, health, damage, hit detection and basic projectile; verify line-of-sight and 3D collision. Leave shields, status effects, advanced beams and deployables for later.

**References:** `COMBAT/COMBAT_ARCHITECTURE_PLAN.md`, `COMBAT/Health.cs`, `COMBAT/Attacks/`, `COMBAT/Projectiles/`, `COMBAT/Hitboxes/`.

---

## Feature 4 — UI notifications (inside the UI umbrella)

**2D origin:** `UI/Notifications/` contains a `NotificationManager`, priority-aware `MajorAlerts/`, queueing/stacking `Toasts/` and resource-backed settings. This is **UI-owned** (not another top-level FEEDBACK system).

### Proposed 3D layout

```text
UI/
├── UiConfig.cs / UiConfig.tres               # Umbrella master
└── Notifications/
    ├── NotificationManager.cs / NotificationManager.tscn
    ├── NotificationSettings.cs / NotificationSettings.tres
    ├── MajorAlerts/
    │   ├── MajorAlertCatalog.cs / MajorAlertCatalog.tres
    │   ├── MajorAlertDefinition.cs
    │   ├── MajorAlertDisplay.cs
    │   └── Definitions/
    │       ├── BiomeDiscovered.tres
    │       ├── HazardDetected.tres
    │       └── BossDetected.tres
    └── Toasts/
        ├── ToastNotification.cs
        └── ToastDisplay.cs
```

**Behaviour worth retaining:** large prioritised threat/discovery banners and small merging pickup/crafting notifications; bounded queues; interruption of critical alerts; user-configurable presentation and durations. 3D is irrelevant to their screen UI layout, but the sources of notifications may come from 3D world events.

**First prototype:** one major alert and one merging toast; wire them to simple debug triggers. Audio alert chimes go through `AUDIO/`, and actor-local damage numbers stay under `FEEDBACK/`.

**References:** `UI/Notifications/README.md`, `UI/Notifications/MajorAlerts/`, `UI/Notifications/Toasts/`.

---

## Cross-system relationship

```text
Player / Enemies / World interactions
               │
               ├─> COMBAT/      authoritative damage, health, impacts
               │      └─events─> FEEDBACK/ actor bars, hit numbers, camera shake
               │
               ├─events───────> AUDIO/    3D SFX, ambience, music, cues
               │
               └─events───────> UI/Notifications/  toasts, alerts
```

The arrows represent **event consumption**, not a requirement to hard-wire these systems to one another. The same combat hit can request feedback and sound without the sound engine modifying health.

## Order and handoff for Codex

| Priority | Scope | Why |
| --- | --- | --- |
| Next | Finish and validate terrain/surface shading and chunk streaming | All actor heights/occlusion depend on stable 3D terrain |
| Early | `AUDIO/` minimal 3D playback foundation | Footsteps, environments and machines will immediately benefit |
| Early | `FEEDBACK/` minimal health/number/shake proof | Establish visual response conventions before many enemies |
| Next | `COMBAT/` basic target and projectile | Exercise 3D damage, signals and occlusion |
| Next | `UI/Notifications/` one alert and one toast | Shared world communication without UI code in gameplay |
| In parallel, focused | Saving/profile foundation already planned in `PROJECT_STRUCTURE.md` | Avoid late persistence retrofit |

**Important:** The root `PROJECT_STRUCTURE.md` also documents world bounds, biomes, `WORLD/Contents/`, terrain/surface appearance, chunks, profiles and saving. Those are **already tracked there**, so this document starts with four additional domains rather than duplicating every roadmap item.

## Later candidates to audit (not specified here yet)

`ENTITIES/` (robots, wildlife, roaming groups, grazing, population), `ITEMS/` and inventory, `SYSTEMS/Crafting/`, `SYSTEMS/Harvesting/`, `SYSTEMS/Interaction/`, `PLAYER/Survival/`, `SYSTEMS/Placement/`, vehicles, interiors/caves, and world events/atmosphere.

**Do not treat the presence of a feature in the 2D repository as proof that it is finished or should be copied exactly.** Revisit the relevant code before planning its 3D implementation.
