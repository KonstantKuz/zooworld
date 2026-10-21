using Core.Actors.Abstract;
using UnityEngine;

namespace Core.Actors.Modules.Abstract
{
    public static class ActorModuleExtensions
    {
        public static bool TryGetPosition(this IActor actor, out Vector3 position)
        {
            if (actor.TryGetModule(out IMovement movement))
            {
                position = movement.Position;
                return true;
            }

            position = default;
            return false;
        }

        public static void ShowTint(this IActor actor, Color tint)
        {
            if (actor.TryGetModule(out IVisual visual))
            {
                visual.ShowTint(tint);
            }
        }

        public static void ClearTint(this IActor actor)
        {
            if (actor.TryGetModule(out IVisual visual))
            {
                visual.ClearTint();
            }
        }

        public static bool IsAlive(this IActor actor)
        {
            return !actor.TryGetModule(out IHealth health) || health.IsAlive;
        }
    }
}
