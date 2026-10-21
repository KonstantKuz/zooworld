using System;
using UnityEngine;

namespace Game.Spawning
{
    [Serializable]
    public sealed class SpawnSettings
    {
        [SerializeField] private float minInterval = 1f;
        [SerializeField] private float maxInterval = 2f;
        [SerializeField] private int maxAnimals = 40;
        [Tooltip("A new animal appears only where nothing stands within this distance.")]
        [SerializeField, Min(0f)] private float freeSpaceRadius = 1.2f;

        public float MinInterval => minInterval;
        public float MaxInterval => maxInterval;
        public int MaxAnimals => maxAnimals;
        public float FreeSpaceRadius => freeSpaceRadius;
    }
}
