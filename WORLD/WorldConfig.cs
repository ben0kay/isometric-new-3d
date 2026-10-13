using Godot;

[GlobalClass]
public partial class WorldConfig : Resource
{
    [ExportGroup("Prototype Terrain")]
    [Export(PropertyHint.Range, "10,500,5")] public float TestGroundSize { get; set; } = 80f;
    [Export] public Color TestGroundColor { get; set; } = new(0.38f, 0.43f, 0.40f);
    [Export(PropertyHint.Range, "0.5,10,0.5")] public float TestGridSpacing { get; set; } = 2f;
    [Export] public bool ShowTestBlocks { get; set; } = true;
}
