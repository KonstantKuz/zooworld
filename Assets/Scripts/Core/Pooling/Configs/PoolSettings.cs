using System;
using UnityEngine;

namespace Core.Pooling.Configs
{
    [Serializable]
    public sealed class PoolSettings
    {
        [Tooltip("Instances created ahead of time for every actor prefab, so spawning never has to allocate.")]
        [SerializeField, Min(0)] private int initialCapacity = 10;
        [Tooltip("More idle instances than this are destroyed instead of kept.")]
        [SerializeField, Min(1)] private int maxIdle = 40;

        public int InitialCapacity => initialCapacity;
        public int MaxIdle => maxIdle;
    }
}
