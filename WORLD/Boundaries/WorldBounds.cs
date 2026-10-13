using Godot;

// One finite boundary policy shared by streaming and physical perimeter walls.
public sealed class WorldBounds
{
    private readonly WorldGenerationSettings _settings;
    public float MinX => _settings.MinX * _settings.ChunkSizeMeters;
    public float MinZ => _settings.MinZ * _settings.ChunkSizeMeters;
    public float MaxX => MinX + _settings.WidthChunks * _settings.ChunkSizeMeters;
    public float MaxZ => MinZ + _settings.LengthChunks * _settings.ChunkSizeMeters;
    public WorldBounds(WorldGenerationSettings settings) { _settings = settings; }

    // =========================================================
    // Reject chunks outside the saved finite rectangle.
    public bool Contains(ChunkCoordinates chunk) =>
        chunk.X >= _settings.MinX && chunk.X < _settings.MinX + _settings.WidthChunks &&
        chunk.Z >= _settings.MinZ && chunk.Z < _settings.MinZ + _settings.LengthChunks;
}
