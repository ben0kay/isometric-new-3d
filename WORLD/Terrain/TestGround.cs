using Godot;

// Flat prototype floor and optional reference blocks; replace when terrain chunks arrive.
public partial class TestGround : StaticBody3D
{
    [Export] public WorldConfig Config { get; set; }

    // Build the floor, its collision, and inexpensive depth references.
    // =========================================================
    public override void _Ready()
    {
        Config ??= GD.Load<WorldConfig>("res://WORLD/WorldConfig.tres");
        float side = Mathf.Max(10, Config.TestGroundSize);
        var floor = GetNode<MeshInstance3D>("MeshInstance3D");
        floor.Mesh = new BoxMesh { Size = new Vector3(side, 0.5f, side) };
        // The flat floor receives shadows but need not cast onto itself.
        floor.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        var material = new ShaderMaterial
        {
            Shader = GD.Load<Shader>("res://WORLD/Terrain/TestGroundGrid.gdshader")
        };
        material.SetShaderParameter("ground_color", Config.TestGroundColor);
        material.SetShaderParameter("grid_spacing", Config.TestGridSpacing);
        floor.MaterialOverride = material;
        GetNode<CollisionShape3D>("CollisionShape3D").Shape =
            new BoxShape3D { Size = new Vector3(side, 0.5f, side) };
        if (!Config.ShowTestBlocks) return;
        var references = new Node3D { Name = "DepthReferences" };
        AddChild(references);
        AddTestBlock(references, new Vector3(-4, 0, -3), new Vector3(2, 1, 2));
        AddTestBlock(references, new Vector3(4, 0, -4), new Vector3(1.5f, 2.5f, 1.5f));
        AddTestBlock(references, new Vector3(5, 0, 3), new Vector3(3, 0.5f, 2));
    }

    // Keep block visuals and collisions together under one grouped node.
    // =========================================================
    private void AddTestBlock(Node3D parent, Vector3 position, Vector3 size)
    {
        var block = new StaticBody3D
        {
            Position = position + new Vector3(0, 0.25f + size.Y * 0.5f, 0)
        };
        parent.AddChild(block);
        block.AddChild(new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.55f, 0.59f, 0.62f), Roughness = 0.9f
            }
        });
        block.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size } });
    }
}
