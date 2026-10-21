using R3;

namespace Core.Actors.Abstract
{
    public interface IActorEvents
    {
        Observable<ActorDeath> Died { get; }
    }
}
