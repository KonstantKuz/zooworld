using Core.Actors.Abstract;
using Core.Actors.Configs;
using R3;
using UnityEngine;
using Zenject;

namespace Core.Actors.Modules
{
    public readonly struct ActorContext
    {
        private readonly Component root;
        private readonly IInstantiator instantiator;

        public ActorContext(IActor owner, ActorConfig config, Component root, CompositeDisposable lifetime,
            IInstantiator instantiator)
        {
            Owner = owner;
            Config = config;
            Lifetime = lifetime;
            this.root = root;
            this.instantiator = instantiator;
        }

        public IActor Owner { get; }
        public ActorConfig Config { get; }
        public CompositeDisposable Lifetime { get; }

        public T Get<T>() where T : Component
        {
            return root.GetComponent<T>();
        }

        public T GetInChildren<T>() where T : Component
        {
            return root.GetComponentInChildren<T>();
        }

        public T Create<T>(params object[] args)
        {
            return instantiator.Instantiate<T>(args);
        }
    }
}
