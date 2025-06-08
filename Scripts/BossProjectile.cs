using Godot;

public partial class BossProjectile : Area3D
{
    [Export] public float Speed {get; set;} = 10f;
    [Export] public float ProjectileLifeTime = 5.0f;
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
        if (body is PlayerShip player)
        {
            player.Controller.TakeDamage(Damage);

        }
        QueueFree();
    }
}
