using Godot;

// Stable signed chunk identity; floor division also handles negative world positions.
public readonly record struct ChunkCoordinates(int X, int Z)
{
    // =========================================================
    // Convert a physical position into its logical chunk.
    public static ChunkCoordinates FromPosition(Vector3 position, int size) =>
        new((int)Mathf.Floor(position.X / size), (int)Mathf.Floor(position.Z / size));
}
