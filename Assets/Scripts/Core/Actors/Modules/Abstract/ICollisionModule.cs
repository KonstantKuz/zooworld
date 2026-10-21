using Core.Actors.Abstract;

namespace Core.Actors.Modules.Abstract
{
    public interface ICollisionModule : IActorModule
    {
        void OnCollided(IActor self, IActor other);
    }
}
