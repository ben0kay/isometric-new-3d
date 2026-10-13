using Godot;

// Builds one indexed heightfield with global sampling and seamless edge normals.
public static class TerrainMeshBuilder
{
    // =========================================================
    // Clockwise triangles face upward in Godot; collision uses this same mesh.
    public static ArrayMesh Build(ChunkCoordinates coordinate, WorldGenerationSettings settings, HeightSampler sampler)
    {
        int cells = settings.CellsPerChunk, row = cells + 1;
        float step = settings.ChunkSizeMeters / (float)cells;
        float originX = coordinate.X * settings.ChunkSizeMeters;
        float originZ = coordinate.Z * settings.ChunkSizeMeters;
        using var surface = new SurfaceTool();
        surface.Begin(Mesh.PrimitiveType.Triangles);
        for (int z = 0; z <= cells; z++)
        for (int x = 0; x <= cells; x++) {
            float worldX = originX + x * step, worldZ = originZ + z * step;
            surface.SetNormal(sampler.Normal(worldX, worldZ));
            surface.SetUV(new Vector2(worldX / 8f, worldZ / 8f));
            surface.AddVertex(new Vector3(x * step, sampler.Sample(worldX, worldZ), z * step));
        }
        for (int z = 0; z < cells; z++)
        for (int x = 0; x < cells; x++) {
            int a = z * row + x, b = a + 1, c = a + row, d = c + 1;
            surface.AddIndex(a); surface.AddIndex(b); surface.AddIndex(c);
            surface.AddIndex(b); surface.AddIndex(d); surface.AddIndex(c);
        }
        return surface.Commit();
    }
}
