using UnityEngine;

namespace Game.Actors.Status
{
    public abstract class ActorStatus
    {
        public abstract string Message { get; }
        public abstract Color Color { get; }
    }
}
