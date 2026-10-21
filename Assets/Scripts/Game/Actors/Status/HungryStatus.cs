using UnityEngine;

namespace Game.Actors.Status
{
    public sealed class HungryStatus : ActorStatus
    {
        public override string Message => "Hungry!";
        public override Color Color => new(1f, 0.55f, 0.1f);
    }
}
