using Godot;

public partial class OuterSpace : Node3D
{
	[Export] public PackedScene PickupScene;
	[Export] public Vector2 PickupXRange = new Vector2(-95, 95);
	[Export] public Vector2 PickupZOffsetRange = new Vector2(200, 400);
	[Export] public float PickupMinAsteroidSpacing = 10f;
	[Export] public float PickupSpawnInterval = 8f;

	private UpgradeMenu menu;
	private PlayerShip ship;
	private EnemySpawnHandler spawner;

	private Timer pickupSpawnTimer;
	private RandomNumberGenerator rng = new RandomNumberGenerator();

	public override void _Ready()
	{
		menu = GetNode<UpgradeMenu>("HeadsUpDisplay/UpgradeMenu");
		ship = GetNode<PlayerShip>("PlayerShip");
		spawner = GetNode<EnemySpawnHandler>("EnemySpawnHandler");

		rng.Randomize();
		pickupSpawnTimer = new Timer
		{
			WaitTime = PickupSpawnInterval,
			OneShot = false
		};
		pickupSpawnTimer.Timeout += OnPickupSpawn;
		AddChild(pickupSpawnTimer);
		pickupSpawnTimer.Start();
	}

	private void OnPickupSpawn()
	{
		Vector3 pos;
		do
		{
			float x = rng.RandfRange(PickupXRange.X, PickupXRange.Y);
			float z = ship.GlobalPosition.Z - rng.RandfRange(PickupZOffsetRange.X, PickupZOffsetRange.Y);
			pos = new Vector3(x, ship.GlobalPosition.Y, z);
		}
		while (!IsPositionOccupied(pos));

		var pickup = PickupScene.Instantiate<Area3D>();
		AddChild(pickup);
		pickup.GlobalPosition = pos;
	}

	private bool IsPositionOccupied(Vector3 pos)
	{
		foreach (var node in GetTree().GetNodesInGroup("Hostile"))
		{
			if (node is Asteroid ast && ast.GlobalPosition.DistanceTo(pos) < PickupMinAsteroidSpacing)
				return false;
		}
		return true;
	}

	private void ApplyHealth()
	{
		new HealthBoost().ApplyTo(ship);
		Resume();
	}

	private void ApplyFireRate()
	{
		new FireRateUpgrade().ApplyTo(ship);
		Resume();
	}

	private void ApplyDamage()
	{
		new DamageUpgrade().ApplyTo(ship);
		Resume();
	}

	private void OnBossSpawned(BossEnemy boss)
	{
		boss.Connect(BossEnemy.SignalName.BossDefeated, new Callable(this, nameof(OnBossDefeated)));
	}

	private void OnBossDefeated()
	{
		GetTree().Paused = true;
		if (menu != null)
			menu.ShowMenu();
	}

	private void Resume()
	{
		menu.HideMenu();
		GetTree().Paused = false;
		spawner.ResetSpawnCycle();
		spawner.AsteroidChance = 1.0f;
		spawner.ActivateKamikazes = true;
	}
}
