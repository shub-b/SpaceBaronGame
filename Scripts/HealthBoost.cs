using Godot;

public class HealthBoost : IPlayerUpgrades
{
    [Export] public float ExtraHealth { get; set; } = 25f;

    public void ApplyTo(PlayerShip ship)
    {
        ship.MaxHealth += ExtraHealth;
        ship.SetCurrentHealth(ship.MaxHealth);
    }
}
