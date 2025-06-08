using Godot;

public partial class Asteroid : StaticBody3D, IEnemy
{
    [Export] public float Damage { get; set; } = 10f;
    [Export] public float MaxHealth { get; set; } = 2f;

    [Export] public int PointsValue { get; set; } = 10;
    [Export] public float DeactivationRange { get; set; } = 10f;

    private float currentHealth;
    private bool active;
    private MeshInstance3D mesh;
    private CollisionShape3D collisionShape;
    private CharacterBody3D playerShip;

    public override void _Ready()
    {
        mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
        playerShip = GetTree().Root.GetNode<CharacterBody3D>("OuterSpace/PlayerShip");
        AddToGroup("Hostile");
        currentHealth = MaxHealth;
        Deactivate();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!active)
            return;

        if (playerShip != null &&
            GlobalPosition.Z > playerShip.GlobalPosition.Z + DeactivationRange)
        {
            Deactivate();
        }
    }

    public bool IsActive() => active;
    public float ApplyDamage() => Damage;

    public void TakeDamage(float amount)
    {
        if (!active)
            return;

        currentHealth -= amount;
        GD.Print($"Asteroid takes {amount} damage, HP now {currentHealth}/{MaxHealth}");

        if (currentHealth <= 0)
            Deactivate();
    }

    public void Activate()
    {
        active = true;
        mesh.Visible = true;
        collisionShape.CallDeferred("set_disabled", false);
        SetPhysicsProcess(true);
    }

    public void Deactivate()
    {
        active = false;
        mesh.Visible = false;
        collisionShape.CallDeferred("set_disabled", true);
        SetPhysicsProcess(false);
    }
}
