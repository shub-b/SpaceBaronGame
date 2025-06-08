using Godot;

public partial class InvincibilityDecorator : PlayerControllerDecorator
{
    private readonly PlayerShip ship;
    private readonly Timer expireTimer;

    public InvincibilityDecorator(IPlayerController previous, float duration, PlayerShip ship)
        : base(previous)
    {
        this.ship = ship;

        expireTimer = new Timer
        {
            WaitTime = duration,
            OneShot = true
        };
        expireTimer.Timeout += OnExpire;
        ship.AddChild(expireTimer);
        expireTimer.Start();
    }

    public override void TakeDamage(float amount)
    {
        return;
    }

    private void OnExpire()
    {
        ship.Controller = wrapper;
        expireTimer.QueueFree();
    }
}
