// res://Scripts/Upgrades/UpgradeMenu.cs
using Godot;
using System;

public partial class UpgradeMenu : Control
{
    [Signal] public delegate void HealthSelectedEventHandler();
    [Signal] public delegate void FireRateSelectedEventHandler();
    [Signal] public delegate void DamageSelectedEventHandler();

    private Button healthButton;
    private Button fireRateButton;
    private Button damageButton;

    private HealthBoost healthUpgrade;
    private FireRateUpgrade fireRateUpgrade;
    private DamageUpgrade damageUpgrade;

    public override void _Ready()
    {
        healthButton = GetNode<Button>("VBoxContainer/HealthButton");
        fireRateButton = GetNode<Button>("VBoxContainer/FireRateButton");
        damageButton = GetNode<Button>("VBoxContainer/DamageButton");

        healthUpgrade = new HealthBoost();
        fireRateUpgrade = new FireRateUpgrade();
        damageUpgrade = new DamageUpgrade();

        float fireRatePct = (1f - fireRateUpgrade.Multiplier) * 100f;
        float damagePct = (damageUpgrade.Multiplier - 1f) * 100f;   

        healthButton.Text = $"Health +{healthUpgrade.ExtraHealth:F0}";
        fireRateButton.Text = $"Fire Rate +{fireRatePct:F0}%";
        damageButton.Text = $"Damage +{damagePct:F0}%";

        healthButton.Pressed += OnHealthPressed;
        fireRateButton.Pressed += OnFireRatePressed;
        damageButton.Pressed += OnDamagePressed;
    }

    private void OnHealthPressed()
    {
        EmitSignal(SignalName.HealthSelected);
        HideMenu();
    }

    private void OnFireRatePressed()
    {
        EmitSignal(SignalName.FireRateSelected);
        HideMenu();
    }

    private void OnDamagePressed()
    {
        EmitSignal(SignalName.DamageSelected);
        HideMenu();
    }

    public void ShowMenu() => Visible = true;
    public void HideMenu() => Visible = false;
}
