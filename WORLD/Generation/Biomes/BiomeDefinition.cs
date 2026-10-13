using Godot;

// Data for a single biome. Generation logic, chunk nodes and collision live elsewhere.
[GlobalClass]
public partial class BiomeDefinition : Resource
{
    [ExportGroup("Identity")]
    [Export] public string Id { get; set; } = "test";
    [Export] public string DisplayName { get; set; } = "Test";
    [Export] public bool Enabled { get; set; } = true;

    [ExportGroup("Terrain")]
    [Export] public TerrainProfile Terrain { get; set; }
    [ExportGroup("Content")]
    [Export] public WorldContentGeneration ContentGeneration { get; set; }
    [ExportGroup("Ground Appearance")]
    [Export] public BiomeSurfaceProfile Surfaces { get; set; }
}


