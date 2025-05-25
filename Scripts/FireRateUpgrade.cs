using Godot;

public partial class FireRateUpgrade : IPlayerUpgrades
{
    [Export] public float Multiplier { get; set; } = 0.8f;

    public void ApplyTo(PlayerShip ship)
    {
        ship.ProjectileFireDelay *= Multiplier;
        var timer = ship.GetNode<Timer>("ShootTimer");
        if (timer != null)
            timer.WaitTime = ship.ProjectileFireDelay;
    }
}