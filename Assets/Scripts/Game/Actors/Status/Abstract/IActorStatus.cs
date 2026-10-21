using Core.Actors.Abstract;
using R3;

namespace Game.Actors.Status.Abstract
{
    public interface IActorStatus
    {
        Observable<ActorStatusChanged> Changed { get; }

        void Set<T>(IActor actor, bool active) where T : ActorStatus, new();
    }
}
