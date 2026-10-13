using Godot;

[GlobalClass]
public partial class WorldConfig : Resource
{
    [ExportGroup("Finite World — Creation Settings")]
    [Export] public int WorldSeed { get; set; } = 727;
    [Export(PropertyHint.Range, "2,128,1")] public int WorldWidthChunks { get; set; } = 32;
    [Export(PropertyHint.Range, "2,128,1")] public int WorldLengthChunks { get; set; } = 32;
    [Export(PropertyHint.Range, "16,128,1")] public int ChunkSizeMeters { get; set; } = 64;
    [Export(PropertyHint.Range, "8,64,1")] public int TerrainCellsPerChunk { get; set; } = 32;
    [Export] public BiomeDefinition PrototypeBiome { get; set; }

    [ExportGroup("Streaming — Runtime Settings")]
    [Export(PropertyHint.Range, "1,5,1")] public int LoadRadiusChunks { get; set; } = 2;
    [Export(PropertyHint.Range, "2,8,1")] public int UnloadRadiusChunks { get; set; } = 3;
    [Export(PropertyHint.Range, "1,4,1")] public int ChunksPerFrame { get; set; } = 1;

    [ExportGroup("Legacy Test Ground / Shared Terrain Colour")]
    [Export(PropertyHint.Range, "10,500,5")] public float TestGroundSize { get; set; } = 80f;
    [Export] public Color TestGroundColor { get; set; } = new(0.38f, 0.43f, 0.40f);
    [Export(PropertyHint.Range, "0.5,10,0.5")] public float TestGridSpacing { get; set; } = 2f;
    [Export] public bool ShowTestBlocks { get; set; } = true;
}
