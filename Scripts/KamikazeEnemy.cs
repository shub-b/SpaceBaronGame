using Godot;
using System;

public partial class KamikazeEnemy : CharacterBody3D, IEnemy
{
	[Export] public float MoveSpeed { get; set; } = 50f;
	[Export] public float MaxHealth { get; set; } = 15f;
	[Export] public int PointsValue { get; set; } = 300;
	[Export] public float Damage { get; set; } = 19f;
	[Export] public PackedScene ProjectileScene { get; set; }
	[Export] public float ShootInterval { get; set; } = 1.0f;
	[Export] public float AttackDuration { get; set; } = 5.0f;
	[Export] public float LookAtPlayerOffset { get; set; } = 1.0f;
	[Export] public float StopLookAtPlayerOffset { get; set; } = 5.0f;
	[Export] public float DeactivationRange { get; set; } = 5.0f;

	private enum State { Attack, Homing }
	private State state;

	private bool active;
	private float currentHealth;
	private CharacterBody3D playerShip;
	private AnimationPlayer anim;
	private Timer shootTimer;
	private Timer attackTimer;
	private CollisionShape3D collider;

	public override void _Ready()
	{
		AddToGroup("Hostile");
		currentHealth = MaxHealth;
		playerShip = GetTree().Root.GetNode<CharacterBody3D>("OuterSpace/PlayerShip");
		collider = GetNode<CollisionShape3D>("CollisionShape3D");
		Scale *= 2;
		anim = GetNode<AnimationPlayer>("KamikazeShipMesh/AnimationPlayer");
		anim.AnimationFinished += name =>
		{
			if (name != "Intro")
				return;

			state = State.Attack;
			active = true;
			SetPhysicsProcess(true);
			shootTimer.Start();
			attackTimer.Start();
		};

		shootTimer = GetNode<Timer>("ShootTimer");
		shootTimer.OneShot = false;
		shootTimer.Autostart = false;
		shootTimer.WaitTime = ShootInterval;
		shootTimer.Timeout += () =>
		{
			if (!active || state != State.Attack) return;
			var proj = ProjectileScene.Instantiate<Area3D>();
			GetParent().AddChild(proj);
			proj.GlobalPosition = GlobalPosition;
			proj.LookAtFromPosition(GlobalPosition, playerShip.GlobalPosition, Vector3.Up);
		};

		attackTimer = GetNode<Timer>("HomingTimer");
		attackTimer.OneShot = true;
		attackTimer.Autostart = false;
		attackTimer.WaitTime = AttackDuration;
		attackTimer.Timeout += () =>
		{
			if (!active || state != State.Attack) return;
			state = State.Homing;
			shootTimer.Stop();
		};

		//Deactivate();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!active) return;

		if (state == State.Attack && GlobalPosition.Z < playerShip.GlobalPosition.Z + 50f)
		{
			LookAt(playerShip.GlobalPosition, Vector3.Up);
			Velocity = new Vector3(0f, 0f, playerShip.Velocity.Z);
		}
		else
		{
			Vector3 moveDir;
			if (GlobalPosition.Z < playerShip.GlobalPosition.Z - StopLookAtPlayerOffset)
			{
				LookAt(playerShip.GlobalPosition, Vector3.Up);
				moveDir = (playerShip.GlobalPosition + Vector3.Forward * LookAtPlayerOffset - GlobalPosition).Normalized();
			}
			else
			{
				moveDir = -GlobalTransform.Basis.Z.Normalized();
			}
			Velocity = moveDir * MoveSpeed;
		}
		MoveAndSlide();
		if (GlobalPosition.Z > playerShip.GlobalPosition.Z + DeactivationRange)
		{
			Deactivate();
		}
	}

	public bool IsActive() => active;
	public float ApplyDamage() => Damage;

	public void TakeDamage(float amount)
	{
		if (!active) return;
		currentHealth -= amount;
		if (currentHealth <= 0f)
			Deactivate();
	}

	public void Activate()
	{
		active = true;
		Show();
		collider.Disabled =  false;
		state = State.Attack;
		SetPhysicsProcess(false);
		anim.Play("Intro");
	}

	public void Deactivate()
	{
		active = false;
		Hide();
		collider.Disabled = true;
		SetPhysicsProcess(false);
		shootTimer.Stop();
		attackTimer.Stop();
		Rotation = Vector3.Zero;
	}
}
