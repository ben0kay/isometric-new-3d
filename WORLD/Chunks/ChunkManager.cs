using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

// Bounded terrain streaming; presentation lifetime is independent of persistent generation settings.
public partial class ChunkManager : Node3D
{
    [Export] public WorldConfig Config { get; set; }
    [Export] public CharacterBody3D Target { get; set; }
    public WorldGenerationSettings GenerationSettings { get; private set; }
    public int LoadedChunkCount => _loaded.Count;
    private readonly Dictionary<ChunkCoordinates, TerrainChunk> _loaded = new();
    private HeightSampler _sampler;
    private WorldBounds _bounds;
    private Material _material;
    private int _loadRadius, _unloadRadius, _budget;
    private bool _ready;

    // =========================================================
    // Future save coordination can supply a validated saved snapshot before entering the scene tree.
    public void UseSavedSettings(WorldGenerationSettings settings)
    {
        if (_ready) throw new InvalidOperationException("Creation settings cannot change in a running world.");
        settings.Validate();
        GenerationSettings = settings;
    }

    // =========================================================
    // Build collision around spawn before the first player physics tick.
    public override void _Ready()
    {
        Config ??= GD.Load<WorldConfig>("res://WORLD/WorldConfig.tres");
        if (Target == null) throw new InvalidOperationException("ChunkManager requires the world player.");
        GenerationSettings ??= WorldGenerationSettings.Capture(Config);
        _sampler = new HeightSampler(GenerationSettings);
        _bounds = new WorldBounds(GenerationSettings);
        _loadRadius = Math.Clamp(Config.LoadRadiusChunks, 1, 5);
        _unloadRadius = Math.Clamp(Config.UnloadRadiusChunks, _loadRadius + 1, 8);
        _budget = Math.Clamp(Config.ChunksPerFrame, 1, 4);
        var surfaces = Config.PrototypeBiome?.Surfaces;
        _material = surfaces != null
            ? GroundSurfaceMaterial.Create(surfaces, GenerationSettings.Seed)
            : new StandardMaterial3D { AlbedoColor = Config.TestGroundColor, Roughness = 1f };
        if (!_bounds.Contains(ChunkCoordinates.FromPosition(Target.GlobalPosition, GenerationSettings.ChunkSizeMeters)))
            throw new InvalidOperationException("Player spawn lies outside the finite world.");
        ProcessPhysicsPriority = -100;
        EnsureNeighborhood(ChunkCoordinates.FromPosition(Target.GlobalPosition, GenerationSettings.ChunkSizeMeters), 1);
        Vector3 spawn = Target.GlobalPosition;
        spawn.Y = Math.Max(spawn.Y, _sampler.Sample(spawn.X, spawn.Z) + 1.05f);
        Target.GlobalPosition = spawn;
        Target.ResetPhysicsInterpolation();
        BuildPerimeter();
        _ready = true;
        GD.Print($"Terrain v{GenerationSettings.GeneratorVersion}: seed {GenerationSettings.Seed}, " +
            $"{GenerationSettings.WidthChunks * GenerationSettings.ChunkSizeMeters} × " +
            $"{GenerationSettings.LengthChunks * GenerationSettings.ChunkSizeMeters} m, biome Test.");
    }

    // =========================================================
    // Safety prefetch before movement: immediate neighboring colliders are always ready.
    public override void _PhysicsProcess(double delta)
    {
        if (!_ready) return;
        Vector3 predicted = Target.GlobalPosition + Target.Velocity * (float)delta;
        EnsureNeighborhood(ChunkCoordinates.FromPosition(predicted, GenerationSettings.ChunkSizeMeters), 1);
    }

    // =========================================================
    // Prioritize nearest missing chunks, then retire distant runtime nodes.
    public override void _Process(double delta)
    {
        if (!_ready) return;
        var center = ChunkCoordinates.FromPosition(Target.GlobalPosition, GenerationSettings.ChunkSizeMeters);
        var missing = new List<ChunkCoordinates>();
        for (int z = -_loadRadius; z <= _loadRadius; z++)
        for (int x = -_loadRadius; x <= _loadRadius; x++) {
            var coordinate = new ChunkCoordinates(center.X + x, center.Z + z);
            if (_bounds.Contains(coordinate) && !_loaded.ContainsKey(coordinate)) missing.Add(coordinate);
        }
        foreach (var coordinate in missing.OrderBy(c =>
            (c.X - center.X) * (c.X - center.X) + (c.Z - center.Z) * (c.Z - center.Z)).Take(_budget))
            EnsureChunk(coordinate);
        foreach (var coordinate in _loaded.Keys.Where(c =>
            Math.Abs(c.X - center.X) > _unloadRadius || Math.Abs(c.Z - center.Z) > _unloadRadius).ToArray()) {
            _loaded[coordinate].QueueFree();
            _loaded.Remove(coordinate);
        }
    }

    // =========================================================
    // Synchronous safety ring is intentionally small; normal streaming is frame-budgeted.
    private void EnsureNeighborhood(ChunkCoordinates center, int radius)
    {
        for (int z = -radius; z <= radius; z++)
        for (int x = -radius; x <= radius; x++)
            EnsureChunk(new ChunkCoordinates(center.X + x, center.Z + z));
    }

    // =========================================================
    // Geometry is deterministic; no scene instance IDs participate in generation.
    private void EnsureChunk(ChunkCoordinates coordinate)
    {
        if (!_bounds.Contains(coordinate) || _loaded.ContainsKey(coordinate)) return;
        var chunk = new TerrainChunk();
        chunk.Build(coordinate, GenerationSettings, _sampler, _material);
        var content = new Node3D { Name = "Content" };
        chunk.AddChild(content);
        if (Config.PrototypeBiome?.Id == GenerationSettings.BiomeId)
            Config.PrototypeBiome.ContentGeneration?.Populate(content, coordinate, GenerationSettings, _sampler);
        AddChild(chunk);
        chunk.ResetPhysicsInterpolation();
        _loaded.Add(coordinate, chunk);
    }

    // =========================================================
    // Temporary invisible physical edges stop the player walking into unloaded void.
    private void BuildPerimeter()
    {
        float width = _bounds.MaxX - _bounds.MinX, length = _bounds.MaxZ - _bounds.MinZ;
        float middleX = (_bounds.MinX + _bounds.MaxX) / 2f;
        float middleZ = (_bounds.MinZ + _bounds.MaxZ) / 2f;
        float height = GenerationSettings.HillHeightMeters * 2f + 200f;
        AddWall(new Vector3(_bounds.MinX - 0.5f, GenerationSettings.BaseHeightMeters, middleZ), new Vector3(1, height, length + 2));
        AddWall(new Vector3(_bounds.MaxX + 0.5f, GenerationSettings.BaseHeightMeters, middleZ), new Vector3(1, height, length + 2));
        AddWall(new Vector3(middleX, GenerationSettings.BaseHeightMeters, _bounds.MinZ - 0.5f), new Vector3(width + 2, height, 1));
        AddWall(new Vector3(middleX, GenerationSettings.BaseHeightMeters, _bounds.MaxZ + 0.5f), new Vector3(width + 2, height, 1));
    }

    // =========================================================
    // Boundary collision is world-owned, independent of chunk retirement.
    private void AddWall(Vector3 position, Vector3 size)
    {
        var body = new StaticBody3D { Position = position };
        body.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size } });
        AddChild(body);
    }
}
