using System.Collections.Generic;
using System;
using Core.Pooling.Configs;
using Object = UnityEngine.Object;
using UnityEngine;
using Zenject;

namespace Core.Pooling
{
    public sealed class ComponentPool
    {
        private readonly Component prefab;
        private readonly IInstantiator instantiator;
        private readonly Transform parent;
        private readonly PoolSettings settings;
        private readonly Stack<Component> idle = new();
        private readonly HashSet<Component> taken = new();

        public ComponentPool(Component prefab, IInstantiator instantiator, Transform parent, PoolSettings settings)
        {
            this.prefab = prefab;
            this.instantiator = instantiator;
            this.parent = parent;
            this.settings = settings;
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Park(Create());
            }
        }

        public Component Get(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            Component instance = idle.Count > 0 ? idle.Pop() : Create();
            if (parent != null)
            {
                instance.transform.SetParent(parent, false);
            }
            else
            {
                instance.transform.SetPositionAndRotation(position, rotation);
            }

            instance.gameObject.SetActive(true);
            taken.Add(instance);
            return instance;
        }

        public bool Release(Component instance)
        {
            if (!taken.Remove(instance))
            {
                throw new InvalidOperationException($"{instance.name} was released twice or does not belong to this pool.");
            }

            if (idle.Count >= settings.MaxIdle)
            {
                Object.Destroy(instance.gameObject);
                return false;
            }

            Park(instance);
            return true;
        }

        private void Park(Component instance)
        {
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(parent, false);
            idle.Push(instance);
        }

        private Component Create()
        {
            return (Component)instantiator.InstantiatePrefabForComponent(
                prefab.GetType(), prefab, parent, Array.Empty<object>());
        }
    }
}
