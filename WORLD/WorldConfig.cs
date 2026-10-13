using Godot;

[GlobalClass]
public partial class WorldConfig : Resource
{
    [ExportGroup("Prototype Terrain")]
    [Export(PropertyHint.Range, "10,500,5")] public float TestGroundSize { get; set; } = 80f;
}
