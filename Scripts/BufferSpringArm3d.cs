using Godot;

public partial class BufferSpringArm3d : SpringArm3D
{
    [Export] public Vector3 Offset        { get; set; } = new Vector3(0, 5, 0);
    [Export] public float   FollowLerp    { get; set; } = 5f;   // how snappy the arm follows
    [Export] public float   MaxSwayAngle  { get; set; } = 10f;  // degrees of roll at full strafe
    [Export] public float   SwayLerp      { get; set; } = 5f;   // how quickly the roll settles

    private PlayerShip _player;

    public override void _Ready()
    {
        _player = GetParent<PlayerShip>();
        if (_player == null)
            GD.PushError("BufferSpringArm3d must be a child of PlayerShip!");
    }

    public override void _Process(double delta)
    {
        if (_player == null) return;
        float dt = (float)delta;

        // 1) Lazy-follow the player on X, Y, Z
        Vector3 targetPos = _player.GlobalPosition + Offset;
        GlobalPosition = GlobalPosition.Lerp(targetPos, FollowLerp * dt);

        // 2) Roll-sway based on strafe velocity
        float strafeVel = _player.Velocity.X;
        float t = _player.MaxStrafeSpeed != 0f
            ? Mathf.Clamp(strafeVel / _player.MaxStrafeSpeed, -1f, 1f)
            : 0f;

        float desiredRoll = -t * MaxSwayAngle;
        var rot = RotationDegrees;
        rot.Z = Mathf.Lerp(rot.Z, desiredRoll, SwayLerp * dt);
        RotationDegrees = rot;
    }
}
