public partial class PlayerControllerDecorator : IPlayerController
{
    protected IPlayerController wrapper;

    protected PlayerControllerDecorator(IPlayerController wrapper)
    {
        this.wrapper = wrapper;
    }

    public virtual void FireProjectile() => wrapper.FireProjectile();
    public virtual void TakeDamage(float a) => wrapper.TakeDamage(a);
    public virtual void RefillHealth() => wrapper.RefillHealth();
}
