using Godot;

public partial class Projectile : Area3D
{
    [Export] public float ProjectileSpeed{get; set;} = 500f;
    [Export] public float ProjectileLifeTime {get; set;} = 8.0f;
    [Export] public float Damage{ get; set; } = 3f;

    public override void _Ready()
    {
        var timer = GetNode<Timer>("Timer");
        timer.WaitTime = ProjectileLifeTime;

        BodyEntered += OnBodyEntered;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        Translate(-GlobalTransform.Basis.Z * ProjectileSpeed * (float)delta);
    }

    private void OnTimerTimeout()
    {
        QueueFree();
    }

    private void OnBodyEntered(Node hitNode)
    {
        if (!hitNode.IsInGroup("Hostile"))
            return;

        if (hitNode is IEnemy enemy)
        {
            enemy.TakeDamage(Damage);
        }
        QueueFree();
    }
}
