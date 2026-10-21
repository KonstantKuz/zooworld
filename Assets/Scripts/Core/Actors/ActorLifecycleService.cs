using System;
using Core.Actors.Abstract;
using Core.Actors.Configs;
using Core.Actors.Modules.Abstract;
using R3;
using UnityEngine;

namespace Core.Actors
{
    public sealed class ActorLifecycleService : IActorLifecycle, IActorEvents, IDisposable
    {
        private readonly IActorFactory factory;
        private readonly Subject<ActorDeath> died = new();

        public ActorLifecycleService(IActorFactory factory)
        {
            this.factory = factory;
        }

        public int ActiveCount => factory.ActiveCount;
        public Observable<ActorDeath> Died => died;

        public IActor Spawn(ActorConfig config, Vector3 position)
        {
            return factory.Create(config, position);
        }

        public void Kill(IActor victim, IActor killer)
        {
            if (!victim.TryGetModule(out IHealth health) || !health.IsAlive)
            {
                return;
            }

            health.Die();
            died.OnNext(new ActorDeath(victim, killer));
            factory.Destroy(victim);
        }

        public void Dispose()
        {
            died.Dispose();
        }
    }
}
