namespace Core.Actors.Modules.Abstract
{
    public interface IHealth : IActorModule
    {
        bool IsAlive { get; }

        void Die();
    }
}
