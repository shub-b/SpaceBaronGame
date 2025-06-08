using Godot;
using System;

public partial class PowerPickup : Area3D
{
    private enum BuffType { Invincibility, FireRate, HealthRefill }

    [Export] public float InvincibilityDuration = 5f;
    [Export] public float FireRateDuration = 5f;
    [Export] public float FireRateMultiplier = 0.5f;

    private BuffType chosenBuff;

    public override void _Ready()
    {
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        chosenBuff = (BuffType)rng.RandiRange(0, Enum.GetValues(typeof(BuffType)).Length - 1);
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is PlayerShip ship)
        {
            switch (chosenBuff)
            {
                case BuffType.Invincibility:
                    ship.Controller = new InvincibilityDecorator(
                        ship.Controller,
                        InvincibilityDuration,
                        ship
                    );
                    GD.Print("INVINCIBILITY ACTIVATED.");
                    break;

                case BuffType.FireRate:
                    ship.Controller = new FireRateDecorator(
                        ship.Controller,
                        FireRateDuration,
                        FireRateMultiplier,
                        ship
                    );
                    GD.Print("DOUBLE FIRE RATE ACTIVATED");
                    break;

                case BuffType.HealthRefill:
                    ship.Controller.RefillHealth();
                    GD.Print("HEALTH REFILLED");
                    break;
            }
            QueueFree();
        }
    }
}
