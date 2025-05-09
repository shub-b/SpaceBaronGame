using Godot;

public partial class HeadsUpDisplay : CanvasLayer
{
    [Export] public NodePath PlayerShipPath { get; set; }

    private PlayerShip player;
    private Control healthBarContainer;
    private ColorRect backgroundBar;
    private ColorRect healthFill;
    private ShaderMaterial shaderMat;

    public override void _Ready()
    {
        player = GetNode<PlayerShip>(PlayerShipPath);
        healthBarContainer = GetNode<Control>("HealthBar");
        backgroundBar = healthBarContainer.GetNode<ColorRect>("BackgroundBar");
        healthFill = healthBarContainer.GetNode<ColorRect>("HealthFill");
        shaderMat = healthFill.Material as ShaderMaterial;
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        if (player == null || healthFill == null || backgroundBar == null || shaderMat == null)
            return;

        float remainingHealth = (float)player.GetCurrentHealth() / player.MaxHealth;
        remainingHealth = Mathf.Clamp(remainingHealth, 0f, 1f);

        float fullBarWidth = backgroundBar.Size.X;
        var resizedBar = healthFill.Size;
        resizedBar.X = fullBarWidth * remainingHealth;
        healthFill.Size = resizedBar;
        
        shaderMat.SetShaderParameter("health", remainingHealth);
    }
}
