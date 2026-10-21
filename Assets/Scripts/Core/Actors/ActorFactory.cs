using System.Collections.Generic;
using Core.Actors.Abstract;
using Core.Actors.Configs;
using Core.Actors.Modules.Abstract;
using Core.Actors.Modules;
using Core.Pooling.Abstract;
using R3;
using UnityEngine;
using Zenject;

namespace Core.Actors
{
    public sealed class ActorFactory : IActorFactory
    {
        private readonly IPoolService pools;
        private readonly IInstantiator instantiator;
        private readonly TickableManager tickables;
        private readonly Dictionary<int, Actor> active = new();
        private readonly List<IActorModule> modules = new();
        private int lastId;

        public ActorFactory(IPoolService pools, IInstantiator instantiator, TickableManager tickables)
        {
            this.pools = pools;
            this.instantiator = instantiator;
            this.tickables = tickables;
        }

        public int ActiveCount => active.Count;

        public IActor Create(ActorConfig config, Vector3 position)
        {
            Actor actor = pools.Get(config.Prefab, position, Quaternion.identity);
            CompositeDisposable lifetime = new CompositeDisposable();

            CreateModules(new ActorContext(actor, config, actor, lifetime, instantiator), config);
            actor.Initialize(++lastId, config, lifetime, modules);

            active[actor.Id] = actor;
            tickables.AddFixed(actor);
            return actor;
        }

        public void Destroy(IActor actor)
        {
            if (active.Remove(actor.Id, out Actor instance))
            {
                tickables.RemoveFixed(instance);
                pools.Release(instance);
            }
        }

        private void CreateModules(ActorContext context, ActorConfig config)
        {
            modules.Clear();
            for (int i = 0; i < config.Modules.Count; i++)
            {
                modules.Add(config.Modules[i].Create(context));
            }
        }
    }
}
