using Godot;

public partial class BossProjectile : Area3D
{
    [Export] public float Speed {get; set;} = 5f;
    [Export] public float ProjectileLifeTime = 7.0f;
    [Export] public float Damage = 10f;

    public override void _Ready()
    {
        var timer = GetNode<Timer>("Timer");
        timer.WaitTime = ProjectileLifeTime;

    }
    
    public override void _PhysicsProcess(double delta)
    {
        Translate(Vector3.Forward * Speed * (float)delta);
    }

    private void OnTimerTimeout()
    {
        QueueFree();
    }

    private void OnBodyEntered(Node body)
    {
        // Only damage the player once
        if (!body.IsInGroup("Player"))
            return;

        // Call the PlayerShip.TakeDamage method
        body.CallDeferred("TakeDamage", Damage);

        // Destroy this projectile
        QueueFree();
    }
}
