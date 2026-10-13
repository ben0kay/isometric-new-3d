using System;
using System.Text.Json;
using Godot;

// Headless integration checks against actual terrain meshes and saved generation snapshots.
public partial class TerrainPrototypeChecks : Node
{
    // =========================================================
    // Run with Godot .NET after building: --headless --path . WORLD/Tests/TerrainPrototypeChecks.tscn
    public override void _Ready()
    {
        try {
            var config = GD.Load<WorldConfig>("res://WORLD/WorldConfig.tres");
            var settings = WorldGenerationSettings.Capture(config);
            Check(ChunkCoordinates.FromPosition(new Vector3(-0.01f, 0, -64.01f), 64) ==
                new ChunkCoordinates(-1, -2), "Negative coordinates use floor division");
            var bounds = new WorldBounds(settings);
            Check(bounds.Contains(new ChunkCoordinates(settings.MinX, settings.MinZ)), "Minimum chunk included");
            Check(!bounds.Contains(new ChunkCoordinates(settings.MinX + settings.WidthChunks, 0)), "Maximum chunk excluded");
            var sampler = new HeightSampler(settings);
            using var left = TerrainMeshBuilder.Build(new ChunkCoordinates(-1, 0), settings, sampler);
            using var right = TerrainMeshBuilder.Build(new ChunkCoordinates(0, 0), settings, sampler);
            var la = left.SurfaceGetArrays(0);
            var ra = right.SurfaceGetArrays(0);
            var lv = la[(int)Mesh.ArrayType.Vertex].AsVector3Array();
            var rv = ra[(int)Mesh.ArrayType.Vertex].AsVector3Array();
            var ln = la[(int)Mesh.ArrayType.Normal].AsVector3Array();
            var rn = ra[(int)Mesh.ArrayType.Normal].AsVector3Array();
            int row = settings.CellsPerChunk + 1;
            for (int z = 0; z < row; z++) {
                int a = z * row + row - 1, b = z * row;
                Check(lv[a].Y == rv[b].Y && ln[a] == rn[b], "Shared edge height and normal");
            }
            Check(right.GetFaces().Length == settings.CellsPerChunk * settings.CellsPerChunk * 6,
                "Complete two-triangle grid");
            using var collision = right.CreateTrimeshShape();
            Check(collision.GetFaces().Length == right.GetFaces().Length, "Collision matches mesh");
            var restored = JsonSerializer.Deserialize<WorldGenerationSettings>(JsonSerializer.Serialize(settings));
            restored.Validate();
            var restoredSampler = new HeightSampler(restored);
            config.WorldSeed += 1; // Resource edits must not change the captured world.
            foreach (float x in new[] { -900f, -64f, 0f, 64f, 700f })
                Check(sampler.Sample(x, 135f) == restoredSampler.Sample(x, 135f), "Snapshot restore reproduces terrain");
            Check(sampler.Sample(0, 0) == settings.BaseHeightMeters, "Safe flat spawn");
            if (settings.MountainEnabled) {
                Check(sampler.Sample(settings.MountainCenterX, settings.MountainCenterZ) >
                    settings.BaseHeightMeters + 40f, "Distinct nearby mountain");
                for (int i = 0; i <= 200; i++) {
                    float t = i / 200f;
                    var normal = sampler.Normal(settings.MountainCenterX * t, settings.MountainCenterZ * t);
                    Check(normal.Y > Mathf.Cos(Mathf.DegToRad(45)), "Spawn-to-mountain route below 45 degrees");
                }
            }
            var legacy = JsonSerializer.Deserialize<WorldGenerationSettings>(JsonSerializer.Serialize(settings));
            legacy.GeneratorVersion = 1;
            var legacyWithMountainFlag = new HeightSampler(legacy);
            float legacyHeight = legacyWithMountainFlag.Sample(-100, -100);
            legacy.MountainEnabled = false;
            var legacyWithoutMountainFlag = new HeightSampler(legacy);
            Check(legacyHeight == legacyWithoutMountainFlag.Sample(-100, -100),
                "Version 1 remains mountain-free");
            restored.GeneratorVersion = 999;
            bool rejected = false;
            try { restored.Validate(); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected, "Unknown generator version rejected");
            GD.Print("Terrain prototype checks passed.");
            GetTree().Quit(0);
        } catch (Exception error) {
            GD.PushError(error.ToString());
            GetTree().Quit(1);
        }
    }

    // =========================================================
    // Fail headless checks with an actionable assertion name.
    private static void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
    }
}
