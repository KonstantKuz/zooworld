using System.Collections.Generic;
using System;
using UnityEngine;

namespace Game.Level.Configs
{
    [Serializable]
    public sealed class LevelSettings
    {
        [SerializeField] private List<ObstacleSlot> slots = new()
        {
            new ObstacleSlot(new Vector2(-4f, 2f), new Vector2(1f, 5f)),
            new ObstacleSlot(new Vector2(5f, -2f), new Vector2(5f, 1f)),
            new ObstacleSlot(new Vector2(0f, 0f), new Vector2(1f, 3f)),
            new ObstacleSlot(new Vector2(-8f, -3f), new Vector2(3f, 1f))
        };

        [Tooltip("How far a wall may be shifted from its home position.")]
        [SerializeField, Min(0f)] private float jitter = 1f;
        [Tooltip("Free space kept around every wall, so animals can always pass between them.")]
        [SerializeField, Min(0f)] private float clearance = 2f;
        [SerializeField, Min(0.1f)] private float height = 1.5f;

        public IReadOnlyList<ObstacleSlot> Slots => slots;
        public float Jitter => jitter;
        public float Clearance => clearance;
        public float Height => height;
    }
}
