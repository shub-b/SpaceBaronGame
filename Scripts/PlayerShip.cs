using Godot;

public partial class PlayerShip : CharacterBody3D
{
	[Export] public PackedScene Projectile { get; set; }
	[Export] public float ProjectileFireDelay { get; set; } = 0.5f;
	[Export] public float ProjectileInstantiationOffsetZ { get; set; } = 1.5f;
	[Export] public float ProjectileScaleMultiplier { get; set; } = 1.0f;
	[Export] public float MaxPlayerSpeed { get; set; } = 35f;
	[Export] public float StrafeAcceleration { get; set; } = 100f;
	[Export] public float MaxStrafeSpeed { get; set; } = 65f;
	[Export] public float XAxisMaxBound { get; set; } = 100f;
	[Export] public float MaxHealth { get; set; } = 300f;

	// Animation for strafing
	private AnimationPlayer strafeAnim;
	[Export] public float StrafeBlendTime { get; set; } = 0.1f;
	private float lastStrafeInput = 0f;

	public IPlayerController Controller { get; set; }
	private AudioStreamPlayer3D laserSound;
	
	private bool active;
	private float currentStrafeSpeed = 0f;
	private float initialXPos;
	private float currentHealth;
	private bool canShoot = true;
	private Timer timer;

	public override void _Ready()
	{
		initialXPos = GlobalPosition.X;
		currentHealth = MaxHealth;
		laserSound = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
		timer = GetNode<Timer>("ShootTimer");
		timer.WaitTime = ProjectileFireDelay;
		timer.Timeout += OnShootTimeout;

		Controller = new BasePlayerController(this);

		// Setup strafe animation
		strafeAnim = GetNode<AnimationPlayer>("PlayerShipMesh/StrafeAnimation");
		strafeAnim.Play("Idle", StrafeBlendTime);
		lastStrafeInput = 0f;
	}

	private void OnShootTimeout()
	{
		canShoot = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		float input = Input.GetActionStrength("strafe_right") - Input.GetActionStrength("strafe_left");

		// Play animation only on input change
		if (input > 0f)
		{
			if (lastStrafeInput <= 0f)
				strafeAnim.Play("StrafeRight", StrafeBlendTime);
		}
		else if (input < 0f)
		{
			if (lastStrafeInput >= 0f)
				strafeAnim.Play("StrafeLeft", StrafeBlendTime);
		}
		else
		{
			if (lastStrafeInput != 0f)
				strafeAnim.Play("Idle", StrafeBlendTime);
		}
		lastStrafeInput = input;

		Velocity = new Vector3(currentStrafeSpeed, 0f, -MaxPlayerSpeed);

		if (Input.IsActionPressed("fire_projectile") && canShoot)
		{
			Controller.FireProjectile();
			laserSound.Play();
			canShoot = false;
			timer.Start();
		}

		StrafeMovement(input, (float)delta);
		MoveAndSlide();
		CheckEnemyCollisions();
		BoundPlayerXAxis();
	}

	// Internal methods called by controllers
	public void InternalFireProjectile()
	{
		if (Projectile == null)
			return;

		var missile = Projectile.Instantiate<Area3D>();
		GetParent().AddChild(missile);
		missile.Scale *= ProjectileScaleMultiplier;
		missile.GlobalPosition = GlobalPosition + Vector3.Forward * ProjectileInstantiationOffsetZ;
	}

	public void InternalTakeDamage(float amount)
	{
		currentHealth -= amount;
		currentHealth = Mathf.Max(currentHealth, 0);
		GD.Print($"Player took {amount} damage, health now {currentHealth}/{MaxHealth}");
		if (currentHealth <= 0)
			OnDeath();
	}

	private void StrafeMovement(float input, float delta)
	{
		if (!Mathf.IsZeroApprox(input))
		{
			currentStrafeSpeed += input * StrafeAcceleration * delta;
			currentStrafeSpeed = Mathf.Clamp(currentStrafeSpeed, -MaxStrafeSpeed, MaxStrafeSpeed);
		}
		else
		{
			currentStrafeSpeed = Mathf.MoveToward(currentStrafeSpeed, 0f, StrafeAcceleration * delta);
		}
	}

	public bool IsActive() => active;

	private void CheckEnemyCollisions()
	{
		int collisionCount = GetSlideCollisionCount();
		for (int i = 0; i < collisionCount; i++)
		{
			var collision = GetSlideCollision(i);
			var collider = collision.GetCollider();
			if (collider is Node3D enemyNode && enemyNode is IEnemy enemy)
			{
				float damage = enemy.ApplyDamage();
				enemy.TakeDamage(damage);
				Controller.TakeDamage(damage);
				break;
			}
		}
	}

	private void BoundPlayerXAxis()
	{
		Vector3 pos = GlobalPosition;
		pos.X = Mathf.Clamp(pos.X, initialXPos - XAxisMaxBound, initialXPos + XAxisMaxBound);
		GlobalPosition = pos;
	}

	private void OnDeath()
	{
		GD.Print("Player has died. Deactivating ship.");
		DeactivatePlayer();
	}

	private void DeactivatePlayer()
	{
		Hide();
		SetProcess(false);
		SetPhysicsProcess(false);
		CollisionLayer = 0;
		CollisionMask = 0;
	}

	public float GetCurrentHealth() => currentHealth;
	public float GetCurrentStrafeSpeed() => currentStrafeSpeed;

	public void SetCurrentHealth(float health)
	{
		currentHealth = health;
	}
}
