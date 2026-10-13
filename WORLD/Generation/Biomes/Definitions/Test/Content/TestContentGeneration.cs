using System;
using Godot;

// Test-only placement strategy and tuning. Terrain/streaming remain shared world services.
[GlobalClass]
public partial class TestContentGeneration : WorldContentGeneration
{
    [Export] public Godot.Collections.Array<PackedScene> Objects { get; set; } = new();
    [Export(PropertyHint.Range, "0,100,1")] public int ObjectsPerChunk { get; set; } = 25;
    [Export(PropertyHint.Range, "0,60,1")] public float MaxSlopeDegrees { get; set; } = 35f;
    [Export(PropertyHint.Range, "0,20,1")] public float SpawnClearanceMeters { get; set; } = 8f;

    // =========================================================
    // Stratified placement avoids large empty patches; global sampling seats props on hills.
    public override void Populate(Node3D parent, ChunkCoordinates chunk,
        WorldGenerationSettings settings, HeightSampler terrain)
    {
        if (Objects.Count == 0) return;
        int count = Math.Clamp(ObjectsPerChunk, 0, 100);
        int grid = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(count)));
        float cell = settings.ChunkSizeMeters / (float)grid;
        float minNormalY = Mathf.Cos(Mathf.DegToRad(Math.Clamp(MaxSlopeDegrees, 0, 60)));
        for (int slot = 0; slot < count; slot++) {
            uint hash = Hash(settings.Seed, chunk.X, chunk.Z, slot);
            float x = (slot % grid + 0.2f + Unit(hash) * 0.6f) * cell;
            float z = (slot / grid + 0.2f + Unit(Mix(hash)) * 0.6f) * cell;
            float worldX = chunk.X * settings.ChunkSizeMeters + x;
            float worldZ = chunk.Z * settings.ChunkSizeMeters + z;
            float clearance = Mathf.Max(0, SpawnClearanceMeters);
            if (worldX * worldX + worldZ * worldZ < clearance * clearance ||
                terrain.Normal(worldX, worldZ).Y < minNormalY) continue;
            int index = (int)(Mix(Mix(hash)) % (uint)Objects.Count);
            AddObject(parent, Objects[index], $"test:{chunk.X}:{chunk.Z}:{slot}",
                new Vector3(x, terrain.Sample(worldX, worldZ), z),
                Unit(Mix(hash ^ 0xAB12CD34u)) * Mathf.Tau);
        }
        // Guaranteed orientation markers near the start, even with a sparse scatter setting.
        PlaceMarker(parent, chunk, settings, terrain, 6, -6, 0);
        PlaceMarker(parent, chunk, settings, terrain, -7, -5, 1);
        PlaceMarker(parent, chunk, settings, terrain, 8, 6, 2);
    }

    // =========================================================
    // Marker ownership uses floor-based signed chunk coordinates.
    private void PlaceMarker(Node3D parent, ChunkCoordinates chunk, WorldGenerationSettings settings,
        HeightSampler terrain, float x, float z, int index)
    {
        if (ChunkCoordinates.FromPosition(new Vector3(x, 0, z), settings.ChunkSizeMeters) != chunk) return;
        AddObject(parent, Objects[index % Objects.Count], $"test:spawn:{index}",
            new Vector3(x - chunk.X * settings.ChunkSizeMeters, terrain.Sample(x, z),
                z - chunk.Z * settings.ChunkSizeMeters), 0);
    }

    // =========================================================
    // Stable metadata is ready for a future removed/harvested-object persistence overlay.
    private static void AddObject(Node3D parent, PackedScene scene, string id, Vector3 position, float yaw)
    {
        if (scene == null) return;
        var instance = scene.Instantiate<Node3D>();
        instance.Position = position;
        instance.Rotation = new Vector3(0, yaw, 0);
        instance.SetMeta("generated_id", id);
        parent.AddChild(instance);
    }

    // =========================================================
    // Stateless hash avoids load-order-dependent random streams.
    private static uint Hash(int seed, int x, int z, int slot)
    {
        unchecked { return Mix((uint)seed ^ (uint)x * 374761393u ^ (uint)z * 668265263u ^ (uint)slot * 2246822519u); }
    }

    // =========================================================
    // Fixed integer mixing is independent of process randomization.
    private static uint Mix(uint value)
    {
        unchecked { value = (value ^ (value >> 16)) * 2246822519u; return value ^ (value >> 13); }
    }

    // =========================================================
    // Keep jitter strictly inside its cell.
    private static float Unit(uint value) => (value & 0xFFFFu) / 65536f;
}
