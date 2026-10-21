using Core.Actors.Abstract;
using Core.Actors.Modules.Abstract;
using UnityEngine;

namespace Game.Actors.Perception
{
    public sealed class NearestActorFinder
    {
        private readonly Collider[] buffer = new Collider[32];

        public bool TryFindNearest<T>(Vector3 origin, float radius, IActor exclude, out IActor nearest) where T : class
        {
            nearest = null;
            float bestDistance = float.MaxValue;
            int count = Physics.OverlapSphereNonAlloc(origin, radius, buffer);
            for (int i = 0; i < count; i++)
            {
                Rigidbody body = buffer[i].attachedRigidbody;
                if (body == null || !body.TryGetComponent(out IActor actor))
                {
                    continue;
                }

                if (actor.Id == exclude.Id || !actor.IsAlive() || !actor.HasTrait<T>() ||
                    !actor.TryGetPosition(out Vector3 position))
                {
                    continue;
                }

                float distance = (position - origin).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = actor;
                }
            }

            return nearest != null;
        }
    }
}
