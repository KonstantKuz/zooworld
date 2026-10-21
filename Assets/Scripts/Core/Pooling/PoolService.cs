using System.Collections.Generic;
using System;
using Core.Pooling.Abstract;
using Core.Pooling.Configs;
using UnityEngine;
using Zenject;

namespace Core.Pooling
{
    public sealed class PoolService : IPoolService
    {
        private readonly IInstantiator instantiator;
        private readonly PoolSettings settings;
        private readonly Dictionary<Component, ComponentPool> poolOfPrefab = new();
        private readonly Dictionary<Component, ComponentPool> poolOfInstance = new();
        private Transform root;

        public PoolService(IInstantiator instantiator, PoolSettings settings)
        {
            this.instantiator = instantiator;
            this.settings = settings;
        }

        public void Prewarm<T>(T prefab, int count) where T : Component
        {
            PoolFor(prefab).Prewarm(count);
        }

        public T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            ComponentPool pool = PoolFor(prefab);
            Component instance = pool.Get(position, rotation, parent);
            poolOfInstance[instance] = pool;
            return (T)instance;
        }

        public void Release<T>(T instance) where T : Component
        {
            if (!poolOfInstance.TryGetValue(instance, out ComponentPool pool))
            {
                throw new InvalidOperationException($"{instance.name} was not taken from a pool.");
            }

            if (!pool.Release(instance))
            {
                poolOfInstance.Remove(instance);
            }
        }

        private ComponentPool PoolFor(Component prefab)
        {
            if (poolOfPrefab.TryGetValue(prefab, out ComponentPool pool))
            {
                return pool;
            }

            if (root == null)
            {
                root = new GameObject("Pools").transform;
            }

            Transform parent = new GameObject($"Pool [{prefab.name}]").transform;
            parent.SetParent(root);
            return poolOfPrefab[prefab] = new ComponentPool(prefab, instantiator, parent, settings);
        }
    }
}
