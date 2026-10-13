# Test biome content prototype

- `ObjectsTest/` owns three collision-enabled placeholders: cyan beacon, tan boulder and orange crate.
- `TestContentGeneration.cs` is the Test biome's unique placement strategy.
- `TestContent.tres` owns its scene list, density, maximum slope and spawn clearance.
- `Test.tres` references that generation resource; shared ChunkManager calls its shared WorldContentGeneration contract.
- No extra chunk manager or terrain generator is created for Test.
- Each chunk owns its Content node, so props retire with terrain.
- Positions derive from seed + signed chunk coordinate + slot, not global random state.
- Three guaranteed props near origin make scale and direction visible immediately.
- Metadata `generated_id` supplies stable IDs for future per-world removed-object overrides.
- This does not yet save removals or snapshot placement settings. Changing density or scene order changes baseline content; lock/version these settings when saving is implemented.
- Godot C# script filename matches class name; the biome-specific generation file is therefore named TestContentGeneration.cs.

Tune ObjectsPerChunk in TestContent.tres. Zero disables random scatter but retains the three start markers; an empty Objects list disables all props.
Walk several chunks away and return to verify identical placements and collision. These are scale references, not final artwork.
