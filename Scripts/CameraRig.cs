using Godot;

public partial class CameraRig : SpringArm3D
{
    [Export] public Vector3 Offset{get; set;} = new Vector3(0, 5, 0);
    [Export] public float FollowLerp{get; set;} = 5f;  
    [Export] public float MaxSwayAngle{get; set;} = 10f;
    [Export] public float SwayLerp {get; set;} = 5f; 

    private PlayerShip playerShip;

    public override void _Ready()
    {
        playerShip = GetParent<PlayerShip>();
    }

    public override void _Process(double delta)
    {
        if (playerShip == null) return;
        Vector3 targetPos = playerShip.GlobalPosition + Offset;
        GlobalPosition = GlobalPosition.Lerp(targetPos, FollowLerp * (float)delta);

        float strafeVel = playerShip.Velocity.X;
        float t = playerShip.MaxStrafeSpeed != 0f? Mathf.Clamp(strafeVel / playerShip.MaxStrafeSpeed, -1f, 1f):0f;

        float cameraRoll = -t * MaxSwayAngle;
        var rotation = RotationDegrees;
        rotation.Z = Mathf.Lerp(rotation.Z, cameraRoll, SwayLerp * (float)delta);
        RotationDegrees = rotation;
    }
}
