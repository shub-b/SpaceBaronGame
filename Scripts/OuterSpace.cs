// res://Scripts/Managers/OuterSpace.cs
using Godot;
using System;

public partial class OuterSpace : Node3D
{
    private UpgradeMenu menu;
    private PlayerShip ship;
    private EnemySpawnHandler spawner;

    public override void _Ready()
    {
        menu = GetNode<UpgradeMenu>("HeadsUpDisplay/UpgradeMenu");
        ship = GetNode<PlayerShip>("PlayerShip");
        spawner = GetNode<EnemySpawnHandler>("EnemySpawnHandler");

        //menu.Connect(UpgradeMenu.SignalName.HealthSelected, new Callable(this, nameof(ApplyHealth)));
        //menu.Connect(UpgradeMenu.SignalName.FireRateSelected, new Callable(this, nameof(ApplyFireRate)));
        //menu.Connect(UpgradeMenu.SignalName.DamageSelected, new Callable(this, nameof(ApplyDamage)));
        //spawner.Connect(EnemySpawnHandler.SignalName.BossSpawned, new Callable(this, nameof(OnBossSpawned)));
    }

    private void ApplyHealth()
    {
        new HealthBoost().ApplyTo(ship);
        Resume();
    }

    private void ApplyFireRate()
    {
        new FireRateUpgrade().ApplyTo(ship);
        Resume();
    }

    private void ApplyDamage()
    {
        new DamageUpgrade().ApplyTo(ship);
        Resume();
    }

    private void OnBossSpawned(BossEnemy boss)
    {
        // Connect the boss’s defeat signal
        boss.Connect(BossEnemy.SignalName.BossDefeated,new Callable(this, nameof(OnBossDefeated)));
    }

    private void OnBossDefeated()
    {
        GetTree().Paused = true;
        if (menu != null)
            menu.ShowMenu();

    }


    private void Resume()
    {
        menu.HideMenu();
        GetTree().Paused = false;
        spawner.ResetSpawnCycle();
        spawner.AsteroidChance = 1.0f;
        spawner.ActivateKamikazes = true;
    }
}
