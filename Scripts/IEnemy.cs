public interface IEnemy
{
    float MaxHealth { get; set; }
    bool IsActive();
    float ApplyDamage();
    void TakeDamage(float damage);
    void Activate();
    void Deactivate();
}

