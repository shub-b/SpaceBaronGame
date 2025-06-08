using Godot;

public partial class BasePlayerController : IPlayerController
{
    private readonly PlayerShip ship;

    public BasePlayerController(PlayerShip ship)
    {
        this.ship = ship;
    }

    public void FireProjectile()
    {
        ship.InternalFireProjectile();
    }

    public void TakeDamage(float amount)
    {
        ship.InternalTakeDamage(amount);
    }

    public void RefillHealth()
    {
        ship.SetCurrentHealth(ship.MaxHealth);
    }
}
