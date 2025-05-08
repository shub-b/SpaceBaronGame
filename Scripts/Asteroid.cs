// Asteroid.cs
using Godot;

public partial class Asteroid : StaticBody3D
{
	[Export] public float RecycleOffset = 10f;

	private bool _active;
	private MeshInstance3D _mesh;
	private CollisionShape3D _collision;

	public override void _Ready()
	{
		_mesh      = GetNode<MeshInstance3D>("MeshInstance3D");
		_collision = GetNode<CollisionShape3D>("CollisionShape3D");
		Deactivate();
	}

	public void Deactivate()
	{
		_active           = false;
		_mesh.Visible     = false;
		_collision.Disabled = true;
		SetPhysicsProcess(false);
	}

	public void Activate()
	{
		_active           = true;
		_mesh.Visible     = true;
		_collision.Disabled = false;
		SetPhysicsProcess(true);
	}

	public bool IsActive() => _active;

	public override void _PhysicsProcess(double delta)
	{
		var player = GetTree().Root
			.GetNode<CharacterBody3D>("OuterSpace/PlayerShip");
		if (GlobalPosition.Z > player.GlobalPosition.Z + RecycleOffset)
			Deactivate();
	}
}
