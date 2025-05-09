public interface IEnemy
{
    bool IsActive();

    float ApplyDamage();
    void Activate();
    void Deactivate();
}

