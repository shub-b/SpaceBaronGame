using Godot;
using System;

public partial class KamikazeEnemy : CharacterBody3D, IEnemy
{
	[Export] public float MoveSpeed {get; set;} = 20f;
	[Export] public float Damage {get; set;} = 50f;
	[Export] public float LookAtZOffset {get; set;} = 1.0f;
	[Export] public float DeactivationRange {get; set;} = 5f;

	private bool active;
	private MeshInstance3D mesh;
	private CollisionShape3D collisionShape;

	public override void _Ready()
	{
		mesh = GetNode<MeshInstance3D>("MeshInstance3D");
		collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
		AddToGroup("Hostile");
		Deactivate();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!active)
			return;
		

		var playerShip = GetTree().Root.GetNode<CharacterBody3D>("OuterSpace/PlayerShip");
		Vector3 moveDirection = (playerShip.GlobalPosition - (Vector3.Back * 0.5f) - GlobalPosition).Normalized();

		float speed = MoveSpeed;
		if (GlobalPosition.Z > playerShip.GlobalPosition.Z)
			speed += playerShip.GlobalPosition.Z - GlobalPosition.Z;

		Velocity = moveDirection * speed;
		MoveAndSlide();

		if (GlobalPosition.Z > playerShip.GlobalPosition.Z + DeactivationRange)
			Deactivate();
	}

	public float ApplyDamage() => Damage;
	public bool  IsActive()     => active;

	public void Activate()
	{
		active                  = true;
		mesh.Visible            = true;
		collisionShape.Disabled = false;
		SetPhysicsProcess(true);
	}

	public void Deactivate()
	{
		active                  = false;
		mesh.Visible            = false;
		collisionShape.Disabled = true;
		SetPhysicsProcess(false);
	}
}
