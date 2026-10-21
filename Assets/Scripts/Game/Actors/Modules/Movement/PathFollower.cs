using System.Collections.Generic;
using UnityEngine;

namespace Game.Actors.Modules.Movement
{
    public sealed class PathFollower
    {
        private const float ArriveRadius = 0.6f;

        private int index;

        public List<Vector3> Corners { get; } = new();

        public bool IsFinished => index >= Corners.Count;

        public void Restart()
        {
            index = 0;
        }

        public void SkipReached(Vector3 position)
        {
            while (index < Corners.Count && IsReached(position))
            {
                index++;
            }
        }

        public Vector3 Direction(Vector3 position)
        {
            if (IsFinished)
            {
                return Vector3.zero;
            }

            Vector3 direction = Corners[index] - position;
            direction.y = 0f;
            return direction.sqrMagnitude > 1e-4f ? direction.normalized : Vector3.zero;
        }

        private bool IsReached(Vector3 position)
        {
            Vector3 corner = Corners[index];
            if (PlanarDistance(position, corner) < ArriveRadius)
            {
                return true;
            }

            if (index == 0)
            {
                return false;
            }

            Vector3 segment = corner - Corners[index - 1];
            Vector3 pastCorner = position - corner;
            segment.y = 0f;
            pastCorner.y = 0f;
            return Vector3.Dot(segment, pastCorner) > 0f;
        }

        private static float PlanarDistance(Vector3 a, Vector3 b)
        {
            a.y = b.y;
            return Vector3.Distance(a, b);
        }
    }
}
