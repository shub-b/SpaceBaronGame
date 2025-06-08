using Godot;

public partial class DamageUpgrade : IPlayerUpgrades
{
    [Export] public float Multiplier { get; set; } = 1.5f;

    public void ApplyTo(PlayerShip ship)
    {
        Projectile.GlobalDamageMultiplier *= Multiplier;
    }
}
