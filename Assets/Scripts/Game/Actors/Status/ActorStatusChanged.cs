using Core.Actors.Abstract;

namespace Game.Actors.Status
{
    public readonly struct ActorStatusChanged
    {
        public ActorStatusChanged(IActor actor, ActorStatus status, bool active)
        {
            Actor = actor;
            Status = status;
            Active = active;
        }

        public IActor Actor { get; }
        public ActorStatus Status { get; }
        public bool Active { get; }
    }
}
