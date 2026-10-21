using System.Collections.Generic;
using Core.Actors.Modules.Movement.Abstract;
using UnityEngine;

namespace Game.Actors.Modules.Movement
{
    public sealed class FixedPathRoute : IRoute
    {
        private const float LookAhead = 3f;
        private const float ArriveDistance = 0.8f;

        private readonly List<Vector3> corners = new();
        private float length;
        private float progress;

        public bool IsFinished { get; private set; } = true;

        public void Follow(List<Vector3> corners)
        {
            this.corners.Clear();
            this.corners.AddRange(corners);
            length = 0f;
            for (int i = 1; i < this.corners.Count; i++)
            {
                length += Vector3.Distance(this.corners[i - 1], this.corners[i]);
            }

            progress = 0f;
            IsFinished = this.corners.Count < 2;
        }

        public void Stop()
        {
            corners.Clear();
            IsFinished = true;
        }

        public Vector3 NextDirection(Vector3 position, float deltaTime)
        {
            if (IsFinished)
            {
                return Vector3.zero;
            }

            progress = Mathf.Max(progress, ProgressOf(position));
            if (length - progress < ArriveDistance)
            {
                IsFinished = true;
                return Vector3.zero;
            }

            Vector3 direction = PointAt(Mathf.Min(progress + LookAhead, length)) - position;
            direction.y = 0f;
            return direction.sqrMagnitude > 1e-4f ? direction.normalized : Vector3.zero;
        }

        private float ProgressOf(Vector3 position)
        {
            float best = progress;
            float bestDistance = float.MaxValue;
            float travelled = 0f;
            for (int i = 1; i < corners.Count; i++)
            {
                Vector3 from = corners[i - 1];
                Vector3 segment = corners[i] - from;
                float segmentLength = segment.magnitude;
                if (travelled + segmentLength >= progress && segmentLength > 1e-4f)
                {
                    float t = Mathf.Clamp01(Vector3.Dot(position - from, segment) / (segmentLength * segmentLength));
                    float distance = Vector3.Distance(position, from + segment * t);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = travelled + t * segmentLength;
                    }
                }

                travelled += segmentLength;
            }

            return best;
        }

        private Vector3 PointAt(float progress)
        {
            float travelled = 0f;
            for (int i = 1; i < corners.Count; i++)
            {
                float segmentLength = Vector3.Distance(corners[i - 1], corners[i]);
                if (travelled + segmentLength >= progress)
                {
                    float t = segmentLength > 1e-4f ? (progress - travelled) / segmentLength : 0f;
                    return Vector3.Lerp(corners[i - 1], corners[i], t);
                }

                travelled += segmentLength;
            }

            return corners[corners.Count - 1];
        }
    }
}
