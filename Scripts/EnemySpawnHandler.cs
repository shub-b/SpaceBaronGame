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

	private CharacterBody3D player;
	private List<Asteroid>  asteroidPool = new();
	private float           lastSpawnZ;
	private RandomNumberGenerator rng = new();

	public override void _Ready()
	{
		player = GetNode<CharacterBody3D>(PlayerPath);
		lastSpawnZ = player.GlobalPosition.Z;
		rng.Randomize();

		// fill pool
		for (int i = 0; i < PoolSize; i++)
		{
			var asteroidObj = (Asteroid)AsteroidScene.Instantiate();
			AddChild(asteroidObj);
			asteroidObj.Deactivate();
			asteroidPool.Add(asteroidObj);
		}

		SetProcess(true);
	}

	public override void _Process(double delta)
	{
		float playerZ = player.GlobalPosition.Z;
		if (lastSpawnZ - playerZ >= SpawnSpace)
		{
			SpawnOne(playerZ);
			lastSpawnZ -= SpawnSpace;
		}
	}

	private void SpawnOne(float playerZ)
	{
		var asteroidObj = asteroidPool.Find(a => !a.IsActive());
		if (asteroidObj == null) return;

		float x = rng.RandfRange(XRange.X, XRange.Y);
		float y = player.GlobalPosition.Y;
		float z = playerZ - SpawnDist;

		asteroidObj.GlobalPosition = new Vector3(x, y, z);
		asteroidObj.Activate();
	}
}
