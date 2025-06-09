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
    [Export] public float BossTriggerDistance { get; set; } = 500f;
    [Export] public float BossOffsetZ { get; set; } = 80f;
    [Export] public float AsteroidChance { get; set; } = 1.0f;
    [Export] public int AsteroidPoolSize { get; set; } = 20;
    [Export] public float AsteroidMinSpawnDistance { get; set; } = 150f;
    [Export] public float AsteroidMaxSpawnDistance { get; set; } = 800f;
    [Export] public float AsteroidSeparationDistance { get; set; } = 10f;
    [Export] public Vector2 AsteroidXSpawnRange { get; set; } = new(-50, 50);
    [Export] public float KamikazeChance { get; set; } = 0.2f;
    [Export] public int KamikazePoolSize { get; set; } = 5;
    [Export] public float KamikazeSpawnDist { get; set; } = 150f;
    [Export] public float KamiKazeSpawnDistDelay { get; set; } = 5f;
    [Export] public Vector2 KamikazeXSpawnRange { get; set; } = new(-95, 95);
    [Export] public NodePath PlayerNode { get; set; }
    [Signal] public delegate void BossSpawnedEventHandler(BossEnemy boss);

    private CharacterBody3D playerShip;
    private List<Asteroid> asteroidPool = new();
    private List<KamikazeEnemy> kamikazePool = new();
    private float lastSpawnZ;
    private float bossTriggerZ;
    private bool bossSpawned = false;
    private RandomNumberGenerator rng = new();

    public int AsteroidTotalInPool => asteroidPool.Count;
    public int AsteroidActiveCount => asteroidPool.Count(a => a.IsActive());
    public int AsteroidInactiveCount => AsteroidTotalInPool - AsteroidActiveCount;
    public int KamikazeTotalInPool => kamikazePool.Count;
    public int KamikazeActiveCount => kamikazePool.Count(k => k.IsActive());

    public override void _Ready()
    {

        AddChild(NullEnemy.Instance);
        NullEnemy.Instance.Hide();
        playerShip = GetNode<CharacterBody3D>(PlayerNode);
        rng.Randomize();

        lastSpawnZ = playerShip.GlobalPosition.Z;
        bossTriggerZ = lastSpawnZ - BossTriggerDistance;
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

        if (!bossSpawned && playerZ <= bossTriggerZ)
        {
            SpawnBoss();
            bossSpawned = true;
            return;
        }
        SpawnSingleEnemyInstance(playerZ);
    }

    private void SpawnBoss()
    {
        var bossNode = BossScene.Instantiate<BossEnemy>();
        AddChild(bossNode);
        bossNode.Scale *= 10.0f;
        var bossSpawnPositionZ = playerShip.GlobalPosition.Z - BossOffsetZ;
        Vector3 spawnPos = new Vector3(0f, 0f, bossSpawnPositionZ);

        bossNode.GlobalPosition = spawnPos;
        EmitSignal(SignalName.BossSpawned, bossNode);
    }

    private void SpawnSingleEnemyInstance(float playerZ)
    {
        float y = playerShip.GlobalPosition.Y;
        if (ActivateAsteroids)
        {
            float x = rng.RandfRange(AsteroidXSpawnRange.X, AsteroidXSpawnRange.Y);
            float zAst = playerZ - rng.RandfRange(AsteroidMinSpawnDistance, AsteroidMaxSpawnDistance);
            Vector3 asteroidPos = new(x, y, zAst);

            if (AsteroidActiveCount < AsteroidPoolSize && IsValidAsteroidPosition(asteroidPos))
                SpawnEnemy(AsteroidScene, asteroidPool, AsteroidPoolSize, AsteroidChance, asteroidPos);
        }

        if (!bossSpawned && ActivateKamikazes && lastSpawnZ - playerZ >= KamikazeSpawnDist)
        {
            Vector3 kamPos = new(
                rng.RandfRange(playerShip.GlobalPosition.X + KamikazeXSpawnRange.X,
                               playerShip.GlobalPosition.X + KamikazeXSpawnRange.Y),
                y, playerZ - KamikazeSpawnDist
            );
            lastSpawnZ -= KamiKazeSpawnDistDelay;
            SpawnEnemy(KamikazeScene, kamikazePool, KamikazePoolSize, KamikazeChance, kamPos);
        }
    }

    private bool IsValidAsteroidPosition(Vector3 pos)
    {
        foreach (var rock in asteroidPool)
        {
            if (rock.IsActive() && rock.GlobalPosition.DistanceTo(pos) < AsteroidSeparationDistance)
                return false;
        }
        return true;
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

        T enemy = pool.FirstOrDefault(e => !e.IsActive());

        if (enemy == null && pool.Count < poolSize)
        {
            enemy = scene.Instantiate<T>();
            AddChild(enemy);
            pool.Add(enemy);
        }

        Node3D node = enemy != null ? enemy : NullEnemy.Instance;
        //node.SetDeferred("global_position", position);

        IEnemy ie = enemy != null ? enemy : NullEnemy.Instance;

        node.GlobalPosition = position;
        ie.Activate();
    }

    public void ResetSpawnCycle()
    {
        bossSpawned = false;
        lastSpawnZ = playerShip.GlobalPosition.Z;
        bossTriggerZ = lastSpawnZ - BossTriggerDistance;
        SetProcess(true);
    }
}
