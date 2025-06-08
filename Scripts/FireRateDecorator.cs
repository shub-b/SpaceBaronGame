using Godot;

public partial class FireRateDecorator : PlayerControllerDecorator
{
    private readonly PlayerShip ship;
    private readonly float originalDelay;
    private readonly Timer expireTimer;

    public FireRateDecorator(IPlayerController baseController, float duration, float multiplier, PlayerShip ship)
        : base(baseController)
    {
        this.ship = ship;
        originalDelay = ship.ProjectileFireDelay;
        ship.ProjectileFireDelay *= multiplier;
        ship.GetNode<Timer>("ShootTimer").WaitTime = ship.ProjectileFireDelay;

        expireTimer = new Timer
        {
            WaitTime = duration,
            OneShot = true
        };
        expireTimer.Timeout += OnExpire;
        ship.AddChild(expireTimer);
        expireTimer.Start();
    }

    private void OnExpire()
    {
        ship.ProjectileFireDelay = originalDelay;
        ship.GetNode<Timer>("ShootTimer").WaitTime = originalDelay;
        ship.Controller = wrapper;
        expireTimer.QueueFree();
    }
}
