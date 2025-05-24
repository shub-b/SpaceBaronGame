using Godot;
using System.Threading.Tasks;

public partial class BossEnemy : CharacterBody3D, IEnemy
{
    [Export] public float Amplitude { get; set; } = 50f;
    [Export] public float Frequency { get; set; } = 0.2f;
    [Export] public PackedScene ProjectileScene { get; set; }
    [Export] public float ShootInterval { get; set; } = 1.5f;
    [Export] public float MuzzleDelay { get; set; } = 0.5f;
    [Export] public float PlayerSeparationDistance { get; set; } = 50f;
    [Export] public int InitialBranches { get; set; } = 4;
    [Export] public float FireArcAngle { get; set; } = 95f;
    [Export] public float MaxHealth { get; set; } = 1000f;
    [Export] public float Damage { get; set; } = 200f;
    [Export] public float HeadDamageMultiplier { get; set; } = 2.5f;
    [Export] public float BodyDamageMultiplier { get; set; } = 1.0f;

    private float time = 0f;
    private float currentHealth;
    private bool active = false;
    private CharacterBody3D playerShip;
    private Timer shootTimer;
    private Node3D[] muzzles;
    private Area3D headZone, bodyZone;

    public override void _Ready()
    {
        AddToGroup("Hostile");

        currentHealth = MaxHealth;
        Activate();

        playerShip = GetTree().Root.GetNode<CharacterBody3D>("OuterSpace/PlayerShip");

        var muzzlePaths = new[] { "Muzzle1", "Muzzle2" };
        muzzles = new Node3D[muzzlePaths.Length];
        for (int i = 0; i < muzzlePaths.Length; i++)
            muzzles[i] = GetNode<Node3D>(muzzlePaths[i]);

        headZone = GetNode<Area3D>("HeadZone");
        bodyZone = GetNode<Area3D>("BodyZone");
        headZone.Monitoring = true;
        bodyZone.Monitoring = true;
        headZone.AreaEntered += body => OnZoneHit(headZone, body);
        bodyZone.AreaEntered += body => OnZoneHit(bodyZone, body);

        if (playerShip != null)
            GlobalPosition = new Vector3(0, GlobalPosition.Y, playerShip.GlobalPosition.Z - PlayerSeparationDistance);

        shootTimer = GetNode<Timer>("ShootTimer");
        shootTimer.WaitTime  = ShootInterval;
        shootTimer.OneShot   = false;
        shootTimer.Autostart = false;
        shootTimer.Timeout  += OnShootTimeout;
        shootTimer.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!active) return;

        time += (float)delta;
        if (playerShip == null || !IsInstanceValid(playerShip))
            return;

        var pos = GlobalPosition;
        pos.X = Mathf.Sin(time * Frequency * Mathf.Tau) * Amplitude;
        GlobalPosition = pos;

        Velocity = new Vector3(0, 0, playerShip.Velocity.Z);
        MoveAndSlide();
    }

    private async void OnShootTimeout()
    {
        if (!active) return;

        shootTimer.Stop();

        foreach (var muzzle in muzzles)
        {
            FireFromMuzzle(muzzle);
            await ToSignal(GetTree().CreateTimer(MuzzleDelay), "timeout");
        }

        if (active)
            shootTimer.Start();
    }

    private void FireFromMuzzle(Node3D muzzle)
    {
        Transform3D xf = muzzle.GlobalTransform;
        Vector3 origin = xf.Origin;
        Vector3 forward = GlobalTransform.Basis.Z;

        float arcRad = Mathf.DegToRad(FireArcAngle);
        float step   = (InitialBranches > 1) ? arcRad / (InitialBranches - 1) : 0f;
        float offset = -arcRad * 0.5f;

        for (int i = 0; i < InitialBranches; i++)
        {
            float angle = offset + step * i;
            Vector3 dir = forward.Rotated(Vector3.Up, angle).Normalized();

            var proj = ProjectileScene.Instantiate<Area3D>();
            proj.GlobalTransform = xf;
            proj.LookAtFromPosition(origin, origin + dir, Vector3.Up);
            GetParent().AddChild(proj);
        }
    }

    private void OnZoneHit(Area3D zone, Node body)
    {
        if (body is Projectile projectile)
        {
            float mult = zone == headZone
                ? HeadDamageMultiplier
                : BodyDamageMultiplier;

            TakeDamage(projectile.Damage * mult);
            projectile.QueueFree();
        }
    }

    public bool IsActive() => active;
    public float ApplyDamage() => Damage;

    public void TakeDamage(float amount)
    {
        if (!active) return;

        currentHealth -= amount;
        GD.Print($"Boss takes {amount} damage, HP now {currentHealth}/{MaxHealth}");
        if (currentHealth <= 0)
            Deactivate();
    }

    public void Activate()
    {
        active = true;
        Show();
        SetPhysicsProcess(true);
        shootTimer?.Start();
    }

    public void Deactivate()
    {
        active = false;
        Hide();
        SetPhysicsProcess(false);
        shootTimer?.Stop();
    }
}
