public interface IEnemy
{
	float MaxHealth { get; set; }

	int PointsValue { get; set;}
	bool IsActive();
	float ApplyDamage();
	void TakeDamage(float damage);
	void Activate();
	void Deactivate();

}
