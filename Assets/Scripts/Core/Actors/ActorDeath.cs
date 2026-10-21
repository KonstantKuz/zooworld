using Core.Actors.Abstract;

namespace Core.Actors
{
    public readonly struct ActorDeath
    {
        public ActorDeath(IActor victim, IActor killer)
        {
            Victim = victim;
            Killer = killer;
        }

        public IActor Victim { get; }
        public IActor Killer { get; }
    }
}
