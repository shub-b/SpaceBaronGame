using Godot;

public partial class Projectile : Area3D
{
    [Export]
    public float Speed { get; set; } = 200f;
    [Export]
    public float ProjectileLifeTime = 8.0f;

    public override void _Ready()
    {
        var timer = GetNode<Timer>("Timer");
        timer.WaitTime = ProjectileLifeTime;

    }
    
    public override void _PhysicsProcess(double delta)
    {
        Translate(Vector3.Forward * Speed * (float)delta);
    }

    private void _OnTimerTimeout()
    {
        QueueFree();
    }

    private void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("Hostile"))
        {
            if (body is IEnemy enemy) 
                ((Node)enemy).CallDeferred("Deactivate");
            QueueFree();
        }
    }
}
