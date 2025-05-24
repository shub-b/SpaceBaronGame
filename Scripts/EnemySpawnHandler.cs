using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class EnemySpawnHandler : Node3D
{
    [Export] public bool ActivateAsteroids { get; set; }
    [Export] public bool ActivateKamikazes { get; set; }
    [Export] public PackedScene AsteroidScene { get; set; }
    [Export] public PackedScene KamikazeScene { get; set; }
    [Export] public PackedScene BossScene { get; set; }
    [Export] public float BossTriggerZ { get; set; } = -1500f;
    [Export] public float BossOffsetZ { get; set; } = 50f;
    [Export] public float AsteroidChance { get; set; } = 1.0f;
    [Export] public float KamikazeChance { get; set; } = 0.2f;
    [Export] public int AsteroidPoolSize { get; set; } = 20;
    [Export] public int KamikazePoolSize { get; set; } = 5;
    [Export] public float SpawnDist { get; set; } = 50f;
    [Export] public float SpawnSpace { get; set; } = 5f;
    [Export] public Vector2 AsteroidXSpawnRange { get; set; } = new(-50, 50);
    [Export] public Vector2 KamikazeXSpawnRange { get; set; } = new(-20, 20);
    [Export] public NodePath PlayerNode { get; set; }

    private CharacterBody3D playerShip;
    private List<Asteroid> asteroidPool = new();
    private List<KamikazeEnemy> kamikazePool = new();
    private float lastSpawnZ;
    private RandomNumberGenerator rng = new();
    private bool bossSpawned = false;

    public int AsteroidTotalInPool => asteroidPool.Count;
    public int AsteroidActiveCount => asteroidPool.Count(e => e.IsActive());
    public int AsteroidInactiveCount => AsteroidTotalInPool - AsteroidActiveCount;
    public int KamikazeTotalInPool => kamikazePool.Count;
    public int KamikazeActiveCount => kamikazePool.Count(e => e.IsActive());

    public override void _Ready()
    {
        playerShip = GetNode<CharacterBody3D>(PlayerNode);
        lastSpawnZ = playerShip.GlobalPosition.Z;
        rng.Randomize();
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        if (!IsInstanceValid(playerShip))
        {
            SetProcess(false);
            return;
        }

        float playerZ = playerShip.GlobalPosition.Z;

        if (!bossSpawned && playerZ <= BossTriggerZ)
        {
            SpawnBoss();
            ActivateAsteroids = false;
            ActivateKamikazes = false;
            bossSpawned = true;
            return;
        }

        if (bossSpawned)
            return;

        if (lastSpawnZ - playerZ >= SpawnSpace)
        {
            SpawnSingleEnemyInstance(playerZ);
            lastSpawnZ -= SpawnSpace;
        }
    }

    private void SpawnBoss()
    {
        var boss = BossScene.Instantiate<Node3D>();
        var parent = GetTree().Root.GetNode<Node3D>("OuterSpace");
        parent.AddChild(boss);
        boss.Scale = new Vector3(4, 4, 4);
        Vector3 pos = playerShip.GlobalPosition;
        pos.Z -= BossOffsetZ;
        boss.GlobalPosition = pos;
    }

    private void SpawnSingleEnemyInstance(float playerZ)
    {
        float y = playerShip.GlobalPosition.Y;
        float z = playerZ - SpawnDist;
        var asteroidPos = new Vector3(
            rng.RandfRange(AsteroidXSpawnRange.X, AsteroidXSpawnRange.Y),
            y, z);
        var kamikazePos = new Vector3(
            rng.RandfRange(playerShip.GlobalPosition.X + KamikazeXSpawnRange.X,
                           playerShip.GlobalPosition.X + KamikazeXSpawnRange.Y),
            y, z);

        if (ActivateAsteroids)
            SpawnEnemy(AsteroidScene, asteroidPool,
                       AsteroidPoolSize, AsteroidChance,
                       asteroidPos);
        if (ActivateKamikazes)
            SpawnEnemy(KamikazeScene, kamikazePool,
                       KamikazePoolSize, KamikazeChance,
                       kamikazePos);
    }

    private void SpawnEnemy<T>(
        PackedScene scene,
        List<T> pool,
        int poolSize,
        float chance,
        Vector3 position
    ) where T : Node3D, IEnemy
    {
        if (scene == null || rng.Randf() >= chance)
            return;

        T enemy = pool.Find(e => !e.IsActive());
        if (enemy == null && pool.Count < poolSize)
        {
            enemy = (T)scene.Instantiate();
            AddChild(enemy);
            enemy.Deactivate();
            pool.Add(enemy);
        }

        if (enemy != null)
        {
            enemy.GlobalPosition = position;
            enemy.Activate();
        }
    }
}
