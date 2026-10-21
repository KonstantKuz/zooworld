using Core.Actors.Abstract;
using Core.Actors.Interactions.Abstract;
using Game.Actors.Traits.Abstract;

namespace Game.Actors.Interactions
{
    public sealed class PredatorDuelRule : InteractionRule<IPredator, IPredator>
    {
        private readonly IActorLifecycle lifecycle;

        public PredatorDuelRule(IActorLifecycle lifecycle)
        {
            this.lifecycle = lifecycle;
        }

        protected override void Apply(IActor first, IActor second)
        {
            IActor winner = first.Id < second.Id ? first : second;
            IActor loser = winner == first ? second : first;
            lifecycle.Kill(loser, winner);
        }
    }
}
