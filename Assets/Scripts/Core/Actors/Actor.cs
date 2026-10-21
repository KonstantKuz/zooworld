using System.Collections.Generic;
using Core.Actors.Abstract;
using Core.Actors.Configs;
using Core.Actors.Modules.Abstract;
using R3;
using UnityEngine;
using Zenject;

namespace Core.Actors
{
    public sealed class Actor : MonoBehaviour, IActor, IFixedTickable
    {
        private readonly List<IActorModule> modules = new();
        private CompositeDisposable lifetime;
        private ActorConfig config;

        private bool IsAlive => lifetime != null && !lifetime.IsDisposed;
        public int Id { get; private set; }

        public void FixedTick()
        {
            if (!IsAlive)
            {
                return;
            }

            foreach (IActorModule module in modules)
            {
                if (module is ITickModule tick)
                {
                    tick.Tick(Time.fixedDeltaTime);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsAlive || collision.rigidbody == null)
            {
                return;
            }

            if (!collision.rigidbody.TryGetComponent(out IActor other))
            {
                return;
            }

            foreach (IActorModule module in modules)
            {
                if (module is ICollisionModule collisionModule)
                {
                    collisionModule.OnCollided(this, other);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !IsAlive)
            {
                return;
            }

            foreach (IActorModule module in modules)
            {
                if (module is IGizmoModule gizmos)
                {
                    gizmos.DrawGizmos();
                }
            }
        }

        public void Initialize(int id, ActorConfig config, CompositeDisposable lifetime,
            IReadOnlyList<IActorModule> modules)
        {
            Id = id;
            this.config = config;
            this.lifetime = lifetime;

            this.modules.Clear();
            this.modules.AddRange(modules);
            foreach (IActorModule module in this.modules)
            {
                if (module is IInitializableModule initializable)
                {
                    initializable.Initialize();
                }
            }
        }

        public bool HasTrait<T>() where T : class
        {
            return config.HasTrait<T>();
        }

        public bool TryGetModule<T>(out T module) where T : class
        {
            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i] is T match)
                {
                    module = match;
                    return true;
                }
            }

            module = null;
            return false;
        }
    }
}
