using Core.Actors.Abstract;
using Core.Actors.Interactions.Abstract;
using Core.Actors.Modules.Abstract;
using Game.Actors.Traits.Abstract;
using UnityEngine;

namespace Game.Actors.Interactions
{
    public sealed class PreyBounceRule : InteractionRule<IPrey, IPrey>
    {
        private readonly float speedChange;

        public PreyBounceRule(float speedChange)
        {
            this.speedChange = speedChange;
        }

        protected override void Apply(IActor first, IActor second)
        {
            Vector3 away = Vector3.zero;
            if (first.TryGetPosition(out Vector3 firstPosition) && second.TryGetPosition(out Vector3 secondPosition))
            {
                away = firstPosition - secondPosition;
                away.y = 0f;
            }

            if (away.sqrMagnitude < 1e-4f)
            {
                Vector2 random = Random.insideUnitCircle.normalized;
                away = new Vector3(random.x, 0f, random.y);
            }

            Vector3 shove = away.normalized * speedChange;
            Push(first, shove);
            Push(second, -shove);
        }

        private static void Push(IActor actor, Vector3 velocityChange)
        {
            if (actor.TryGetModule(out IPushable pushable))
            {
                pushable.Push(velocityChange);
            }
        }
    }
}
