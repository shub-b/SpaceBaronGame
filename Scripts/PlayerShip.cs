using Godot;

public partial class PlayerShip : CharacterBody3D
{
	[Export] PackedScene Projectile {get; set;}
	[Export] public float MaxPlayerSpeed { get; set; } = 25f;
	[Export] public float StrafeAcceleration { get; set; } = 30f;
	[Export] public float MaxStrafeSpeed { get; set; } = 25f;      

	[Export] public float XAxisMaxBound {get; set;} = 30f;    

	private float strafeSpeed = 0f;
	private float initialXPos;

	public override void _Ready()
	{
		// Remember the starting X position
		initialXPos = GlobalPosition.X;
	}

	public override void _PhysicsProcess(double delta)
	{
		float deltaFloat = (float)delta;		
		float input = Input.GetActionStrength("strafe_right")- Input.GetActionStrength("strafe_left");
		Velocity = new Vector3(strafeSpeed, 0f, -MaxPlayerSpeed);

		if (Input.IsActionJustPressed("fire_projectile")){
			FireProjectile();
		}

		StrafeMovement(input, deltaFloat);

		MoveAndSlide();
		BoundPlayerXAxis();


	}

	void StrafeMovement(float input, float delta){


		if (!Mathf.IsZeroApprox(input))
		{
			strafeSpeed += input * StrafeAcceleration * delta;
			strafeSpeed = Mathf.Clamp(strafeSpeed, -MaxStrafeSpeed, MaxStrafeSpeed);
		}
		else
		{
			strafeSpeed = Mathf.MoveToward(strafeSpeed, 0f, StrafeAcceleration * delta);
		}
	}

	void FireProjectile()
	{
		if (Projectile == null)
			return;

		var missile = Projectile.Instantiate<Area3D>();
		GetParent().AddChild(missile);
		missile.Scale = new Vector3(0.25f, 0.25f, 0.25f);
		missile.GlobalPosition = GlobalPosition;
	}

	void BoundPlayerXAxis(){
		var pos = GlobalPosition;
		pos.X = Mathf.Clamp(pos.X, initialXPos - XAxisMaxBound, initialXPos + XAxisMaxBound);
		GlobalPosition = pos;
	}
}
