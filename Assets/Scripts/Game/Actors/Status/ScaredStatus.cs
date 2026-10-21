using UnityEngine;

namespace Game.Actors.Status
{
    public sealed class ScaredStatus : ActorStatus
    {
        public override string Message => "Scary!";
        public override Color Color => new(1f, 0.95f, 0.3f);
    }
}
