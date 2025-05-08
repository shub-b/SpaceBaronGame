// EnemySpawnHandler.cs
using Godot;
using System.Collections.Generic;

public partial class EnemySpawnHandler : Node3D
{
    [Export] public PackedScene AsteroidScene { get; set; }
    [Export] public int PoolSize         = 20;
    [Export] public float SpawnDist      = 50f;
    [Export] public float SpawnSpace     = 5f;       // distance (Z) between spawns — make smaller to spawn faster
    [Export] public Vector2 XRange       = new(-8, 8);
    [Export] public NodePath PlayerPath;

    private CharacterBody3D _player;
    private List<Asteroid>  _pool = new();
    private float           _lastSpawnZ;
    private RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        _player     = GetNode<CharacterBody3D>(PlayerPath);
        _lastSpawnZ = _player.GlobalPosition.Z;
        _rng.Randomize();

        // fill pool
        for (int i = 0; i < PoolSize; i++)
        {
            var ast = (Asteroid)AsteroidScene.Instantiate();
            AddChild(ast);    // let its _Ready() cache mesh & collision
            ast.Deactivate();
            _pool.Add(ast);
        }

        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        float playerZ = _player.GlobalPosition.Z;
        if (_lastSpawnZ - playerZ >= SpawnSpace)
        {
            SpawnOne(playerZ);
            _lastSpawnZ -= SpawnSpace;
        }
    }

    private void SpawnOne(float playerZ)
    {
        var ast = _pool.Find(a => !a.IsActive());
        if (ast == null) return;  // pool exhausted

        float x = _rng.RandfRange(XRange.X, XRange.Y);
        float y = _player.GlobalPosition.Y;
        float z = playerZ - SpawnDist;

        ast.GlobalPosition = new Vector3(x, y, z);
        ast.Activate();
    }
}
