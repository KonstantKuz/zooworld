using System;
using UnityEngine;

namespace Game.Level.Configs
{
    [Serializable]
    public sealed class ObstacleSlot
    {
        [SerializeField] private Vector2 position;
        [SerializeField] private Vector2 size = Vector2.one;

        public ObstacleSlot()
        {
        }

        public ObstacleSlot(Vector2 position, Vector2 size)
        {
            this.position = position;
            this.size = size;
        }

        public Vector2 Position => position;
        public Vector2 Size => size;
    }
}
