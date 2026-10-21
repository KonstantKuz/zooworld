using System;
using Core.Actors.Abstract;
using Core.Actors;
using Game.Actors.Traits.Abstract;
using R3;
using Zenject;

namespace Game.Stats
{
    public sealed class KillStatsService : IInitializable, IDisposable
    {
        private readonly IActorEvents events;
        private readonly ReactiveProperty<int> deadPrey = new(0);
        private readonly ReactiveProperty<int> deadPredators = new(0);
        private IDisposable subscription;

        public KillStatsService(IActorEvents events)
        {
            this.events = events;
        }

        public ReadOnlyReactiveProperty<int> DeadPrey => deadPrey;
        public ReadOnlyReactiveProperty<int> DeadPredators => deadPredators;

        public void Initialize()
        {
            subscription = events.Died.Subscribe(OnDied);
        }

        public void Dispose()
        {
            subscription?.Dispose();
            deadPrey.Dispose();
            deadPredators.Dispose();
        }

        private void OnDied(ActorDeath death)
        {
            if (death.Victim.HasTrait<IPrey>())
            {
                deadPrey.Value++;
            }
            else if (death.Victim.HasTrait<IPredator>())
            {
                deadPredators.Value++;
            }
        }
    }
}
