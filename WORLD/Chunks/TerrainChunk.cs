using Godot;

// Owns generated mesh and matching static collision; retiring this node frees both.
public partial class TerrainChunk : Node3D
{
    // =========================================================
    // Activate a complete chunk, never a visual-only floor.
    public void Build(ChunkCoordinates coordinate, WorldGenerationSettings settings,
        HeightSampler sampler, Material material)
    {
        Name = $"Chunk_{coordinate.X}_{coordinate.Z}";
        Position = new Vector3(coordinate.X * settings.ChunkSizeMeters, 0, coordinate.Z * settings.ChunkSizeMeters);
        var mesh = TerrainMeshBuilder.Build(coordinate, settings, sampler);
        AddChild(new MeshInstance3D { Mesh = mesh, MaterialOverride = material });
        var body = new StaticBody3D();
        body.AddChild(new CollisionShape3D { Shape = mesh.CreateTrimeshShape() });
        AddChild(body);
    }
}
