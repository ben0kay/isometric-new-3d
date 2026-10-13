using Godot;

// Prototype-only geometry. Replace with a terrain provider when chunks arrive.
public partial class TestGround : StaticBody3D
{
    [Export] public WorldConfig Config { get; set; }

    public override void _Ready()
    {
        Config ??= GD.Load<WorldConfig>("res://WORLD/WorldConfig.tres");
        float side = Mathf.Max(10, Config.TestGroundSize);
        GetNode<MeshInstance3D>("MeshInstance3D").Mesh = new BoxMesh { Size = new Vector3(side, 0.5f, side) };
        GetNode<CollisionShape3D>("CollisionShape3D").Shape =
            new BoxShape3D { Size = new Vector3(side, 0.5f, side) };
    }
}
