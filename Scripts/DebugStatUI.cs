using Godot;

public partial class DebugStatUI : CanvasLayer
{
    [Export] public NodePath PlayerNode  { get; set; }
    [Export] public NodePath SpawnerNode { get; set; }
    private PlayerShip playerShip;
    private EnemySpawnHandler enemySpawner;
    private Label playerZLabel;
    private Label asteroidMetricsLabel;
    private Label kamikazeMetricsLabel;
    private Label strafeSpeedLabel;

    public override void _Ready()
    {
        playerShip  = GetNode<PlayerShip>(PlayerNode);
        enemySpawner = GetNode<EnemySpawnHandler>(SpawnerNode);

        var vbox = GetNode<VBoxContainer>("VBoxContainer");
        playerZLabel = vbox.GetNode<Label>("PlayerZLabel");
        asteroidMetricsLabel = vbox.GetNode<Label>("AsteroidMetricsLabel");
        kamikazeMetricsLabel = vbox.GetNode<Label>("KamikazeMetricsLabel");
        strafeSpeedLabel = vbox.GetNode<Label>("StrafeSpeedLabel");

        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        playerZLabel.Text = $"Player Z: {playerShip.GlobalPosition.Z:F1}";
        strafeSpeedLabel.Text= $"Strafe Speed: {playerShip.GetCurrentStrafeSpeed().ToString()}";
        asteroidMetricsLabel.Text = $"Asteroids(active/pooled): {enemySpawner.AsteroidActiveCount}/{enemySpawner.AsteroidTotalInPool}";
        kamikazeMetricsLabel.Text = $"Kamikazes(active/pooled): {enemySpawner.KamikazeActiveCount}/{enemySpawner.KamikazeTotalInPool}";

    }
}
