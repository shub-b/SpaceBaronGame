using Godot;

public partial class NullEnemy : Node3D, IEnemy
{
	public static readonly NullEnemy Instance = new NullEnemy();
	private NullEnemy() { }

	public float MaxHealth { get; set; } = 0f;
	public int PointsValue { get; set; } = 0;

    public bool IsActive() => false;
	public void Activate() {}
	public void Deactivate() {}
	public float ApplyDamage() => 0f;
	public void TakeDamage(float amount) {}
}
