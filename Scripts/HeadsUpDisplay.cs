using Godot;

public partial class HeadsUpDisplay : CanvasLayer
{
    [Export] public NodePath PlayerShipPath { get; set; }

    private PlayerShip player;
    private Control healthBarContainer;
    private ColorRect backgroundBar;
    private ColorRect healthFill;
    private ShaderMaterial shaderMat;
    private Control playerScoreContainer;
    private Label scoreLabel;
    private Label damageDoneLabel;

    private int score;
    private int killScore;
    private int runningScore;

    public override void _Ready()
    {
        player = GetNode<PlayerShip>(PlayerShipPath);
        healthBarContainer = GetNode<Control>("HealthBar");
        playerScoreContainer = GetNode<Control>("PlayerScore");
        scoreLabel = playerScoreContainer.GetNode<Label>("PlayerScoreLabel");
        backgroundBar = healthBarContainer.GetNode<ColorRect>("BackgroundBar");
        healthFill = healthBarContainer.GetNode<ColorRect>("HealthFill");
        shaderMat = healthFill.Material as ShaderMaterial;
        damageDoneLabel = GetNode<Label>("DamageDone/DamageDoneLabel");
        score = 0;
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

        runningScore = (int)Mathf.Abs(player.GlobalPosition.Z);
        score = runningScore + killScore;
        scoreLabel.Text = $"BOUNTY: ${score}";
    }

    public void AddScore(int points)
    {
        killScore += points;
    }

    public void ShowDamage(float damage)
    {
        damageDoneLabel.Text = $"DMG: {damage:F2}";
        damageDoneLabel.Modulate = new Color(1, 1, 1, 0);
        damageDoneLabel.Scale = Vector2.One;
        damageDoneLabel.Show();

        var tween = damageDoneLabel.CreateTween();


        tween.TweenProperty(
                damageDoneLabel, "scale", new Vector2(1.3f, 1.3f), 0.1f
            ).SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(
            damageDoneLabel, "scale", Vector2.One, 0.1f
        ).SetDelay(0.1f)
         .SetTrans(Tween.TransitionType.Sine)
         .SetEase(Tween.EaseType.In);

        tween.TweenProperty(
            damageDoneLabel, "modulate:a", 1f, 0f
        ).SetTrans(Tween.TransitionType.Sine)
         .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(
            damageDoneLabel, "modulate:a", 0f, 2.5f
        ).SetDelay(2.0f)
         .SetTrans(Tween.TransitionType.Sine)
         .SetEase(Tween.EaseType.Out);

        tween.TweenCallback(Callable.From(() => damageDoneLabel.Hide()));
    }



}
