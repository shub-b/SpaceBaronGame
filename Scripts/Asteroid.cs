// Asteroid.cs
using Godot;

public partial class Asteroid : StaticBody3D, IEnemy
{
	[Export] public float Damage = 10;
	
	[Export] public float DeactivationRage = 10f; 
	private bool active;
	private MeshInstance3D mesh;
	private CollisionShape3D collision;

	public override void _Ready()
	{
		mesh = GetNode<MeshInstance3D>("MeshInstance3D");
		collision = GetNode<CollisionShape3D>("CollisionShape3D");
		Deactivate();
	}

	public void Deactivate()
	{
		active = false;
		mesh.Visible = false;
		collision.Disabled = true;
		SetPhysicsProcess(false);
	}

	public void Activate()
	{
		active = true;
		mesh.Visible = true;
		collision.Disabled = false;
		SetPhysicsProcess(true);
	}

	public bool IsActive() => active;

	 public float ApplyDamage() => Damage;

	public override void _PhysicsProcess(double delta)
	{
		var playerShip = GetTree().Root
			.GetNode<CharacterBody3D>("OuterSpace/PlayerShip");
		if (GlobalPosition.Z > playerShip.GlobalPosition.Z + DeactivationRage)
			Deactivate();
	}
}
