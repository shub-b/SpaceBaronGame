using Godot;

public partial class BossEnemy : CharacterBody3D, IEnemy
{
    [Export] public float Amplitude { get; set; } = 50f;
    [Export] public float Frequency { get; set; } = 0.2f;
    [Export] public PackedScene ProjectileScene { get; set; }
    [Export] public float ShootInterval { get; set; } = 1.5f;
    [Export] public float MuzzleDelay { get; set; } = 0.5f;
    [Export] public float PlayerSeparationDistance { get; set; } = 50f;
    [Export] public int ProjectileQuantity { get; set; } = 3;
    [Export] public float FireArcAngle { get; set; } = 120f;
    [Export] public float MaxHealth { get; set; } = 1000f;
    [Export] public float Damage { get; set; } = 200f;
    [Export] public float HeadDamageMultiplier { get; set; } = 2.5f;
    [Export] public float BodyDamageMultiplier { get; set; } = 1.0f;
    [Export] public ShaderMaterial PulseMaterial { get; set; }
    [Export] public ShaderMaterial HealthBarMaterial { get; set; }
    [Export] public float PulseDuration { get; set; } = 0.8f;
    [Signal] public delegate void BossDefeatedEventHandler();

    private float pulseTimer = 0f;
    private bool pulsing = false;
    private float time = 0f;
    private float currentHealth;
    private bool active = false;
    private CharacterBody3D playerShip;
    private Timer shootTimer;
    private Node3D[] muzzles;
    private Area3D headZone, bodyZone;
    private MeshInstance3D headControlMesh;

    public override void _Ready()
    {
        AddToGroup("Hostile");
        currentHealth = MaxHealth;
        Activate();

        playerShip = GetTree().Root.GetNode<CharacterBody3D>("OuterSpace/PlayerShip");

        muzzles =
        [
            GetNode<Node3D>("FiringMuzzles/Muzzle1"),
            GetNode<Node3D>("FiringMuzzles/Muzzle2")
        ];

        headZone = GetNode<Area3D>("CollisionZones/HeadZone");
        bodyZone = GetNode<Area3D>("CollisionZones/BodyZone");
        headZone.Monitoring = true;
        bodyZone.Monitoring = true;
        headZone.AreaEntered += (body) => OnZoneHit(headZone, body);
        bodyZone.AreaEntered += (body) => OnZoneHit(bodyZone, body);
        ApplyPulseShaderToBody();

        headControlMesh = GetNode<MeshInstance3D>("BossShipMesh/BossHead/BossControlMesh3D");
        headControlMesh.MaterialOverride = HealthBarMaterial;
        HealthBarMaterial.SetShaderParameter("health_percent", currentHealth / MaxHealth);

        PositionBehindPlayer();

        shootTimer = GetNode<Timer>("ShootTimer");
        shootTimer.WaitTime = ShootInterval;
        shootTimer.OneShot = false;
        shootTimer.Autostart = false;
        shootTimer.Timeout += OnShootTimeout;
        shootTimer.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!active)
            return;

        time += (float)delta;
        if (playerShip == null || !IsInstanceValid(playerShip))
            return;

        HandlePulse(delta);
        UpdateHealthBar();
        SwayMovement();
        FollowPlayerZ();
    }

    private void ApplyPulseShaderToBody()
    {
        Node3D bodyRoot = GetNode<Node3D>("BossShipMesh/BossBody");
        foreach (Node child in bodyRoot.GetChildren())
            if (child is MeshInstance3D mesh)
                mesh.MaterialOverride = PulseMaterial;
        PulseMaterial.SetShaderParameter("pulse_time", 0f);
    }

    private void PositionBehindPlayer()
    {
        if (playerShip != null)
            GlobalPosition = new Vector3(
                0f,
                GlobalPosition.Y,
                playerShip.GlobalPosition.Z - PlayerSeparationDistance
            );
    }

    private void HandlePulse(double delta)
    {
        if (!pulsing) return;
        pulseTimer += (float)delta;
        float pulseTime;
        if (pulseTimer <= PulseDuration)
            pulseTime = pulseTimer / PulseDuration;
        else if (pulseTimer <= PulseDuration * 2f)
            pulseTime = 1f - ((pulseTimer - PulseDuration) / PulseDuration);
        else
        {
            pulseTime = 0f;
            pulsing = false;
        }
        PulseMaterial.SetShaderParameter("pulse_time", pulseTime);
    }

    private void UpdateHealthBar()
    {
        float hpPct = Mathf.Clamp(currentHealth / MaxHealth, 0f, 1f);
        HealthBarMaterial.SetShaderParameter("health_percent", hpPct);
    }

    private void SwayMovement()
    {
        Vector3 pos = GlobalPosition;
        pos.X = Mathf.Sin(time * Frequency * Mathf.Tau) * Amplitude;
        GlobalPosition = pos;
    }

    private void FollowPlayerZ()
    {
        Velocity = new Vector3(0f, 0f, playerShip.Velocity.Z);
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
        if (active) shootTimer.Start();
    }

    private void FireFromMuzzle(Node3D muzzle)
    {
        Transform3D xf = muzzle.GlobalTransform;
        Vector3 origin = xf.Origin;
        Vector3 forward = GlobalTransform.Basis.Z;

        float arcRad = Mathf.DegToRad(FireArcAngle);
        float step = (ProjectileQuantity > 1) ? arcRad / (ProjectileQuantity - 1) : 0f;
        float offset = -arcRad * 0.5f;

        for (int i = 0; i < ProjectileQuantity; i++)
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
        if (body is Projectile proj)
        {
            Vector3 color = zone == headZone ? new Vector3(1f, 0f, 0f) : new Vector3(1f, 0.5f, 0f);
            PulseMaterial.SetShaderParameter("highlight_color", color);
            pulseTimer = 0f; pulsing = true;
            PulseMaterial.SetShaderParameter("pulse_time", 0f);
            TakeDamage(proj.Damage * Projectile.GlobalDamageMultiplier * (zone == headZone ? HeadDamageMultiplier : BodyDamageMultiplier));
            proj.QueueFree();
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
            Die();
    }

    private void Die()
    {
        EmitSignal(SignalName.BossDefeated);
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