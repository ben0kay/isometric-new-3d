using Godot;

// Shared strategy contract: biomes author placement; chunks own the resulting objects.
public abstract partial class WorldContentGeneration : Resource
{
    // =========================================================
    // Implementations use saved seed and signed chunk IDs, never runtime instance IDs.
    public abstract void Populate(Node3D parent, ChunkCoordinates chunk,
        WorldGenerationSettings settings, HeightSampler terrain);
}
