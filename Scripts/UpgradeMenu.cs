// res://Scripts/Upgrades/UpgradeMenu.cs
using Godot;
using System;

public partial class UpgradeMenu : Control
{
    // 1) Declare three Godot signals
    [Signal] public delegate void HealthSelectedEventHandler();
    [Signal] public delegate void FireRateSelectedEventHandler();
    [Signal] public delegate void DamageSelectedEventHandler();

    public override void _Ready()
    {
        // 2) Hook each button’s pressed() to a local handler
        GetNode<Button>("VBoxContainer/HealthButton").Pressed += OnHealthPressed;
        GetNode<Button>("VBoxContainer/FireRateButton").Pressed += OnFireRatePressed;
        GetNode<Button>("VBoxContainer/DamageButton").Pressed += OnDamagePressed;
    }

    // 3) In each handler, emit the corresponding signal, then hide
    private void OnHealthPressed()
    {
        GD.Print("[UpgradeMenu] Health button pressed");
        EmitSignal(SignalName.HealthSelected);
        HideMenu();
    }

    private void OnFireRatePressed()
    {
        GD.Print("[UpgradeMenu] FireRates button pressed");
        EmitSignal(SignalName.FireRateSelected);
        HideMenu();
    }

    private void OnDamagePressed()
    {
        GD.Print("[UpgradeMenu] Damage button pressed");
        EmitSignal(SignalName.DamageSelected);
        HideMenu();
    }

    // 4) Utility methods to show/hide
    public void ShowMenu() => Visible = true;
    public void HideMenu() => Visible = false;
}
