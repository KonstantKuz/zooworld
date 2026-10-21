using Core.Actors.Abstract;
using Core.Actors.Interactions.Abstract;
using Game.Actors.Traits.Abstract;

namespace Game.Actors.Interactions
{
    public sealed class PredatorEatsPreyRule : InteractionRule<IPredator, IPrey>
    {
        private readonly IActorLifecycle lifecycle;

        public PredatorEatsPreyRule(IActorLifecycle lifecycle)
        {
            this.lifecycle = lifecycle;
        }

        protected override void Apply(IActor predator, IActor prey)
        {
            lifecycle.Kill(prey, predator);
        }
    }
}
