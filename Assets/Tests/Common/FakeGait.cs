using Core.Actors.Modules.Movement.Abstract;
using Core.Actors.Modules;

namespace Tests
{
    /// <summary>Movement config for tests that only need a gait identity, not real physics.</summary>
    public sealed class FakeGait : MovementConfig
    {
        public override IMovementStrategy Create(ActorContext context)
        {
            return null;
        }
    }
}
