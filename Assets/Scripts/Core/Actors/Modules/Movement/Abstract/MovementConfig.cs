using UnityEngine;

namespace Core.Actors.Modules.Movement.Abstract
{
    public abstract class MovementConfig : ScriptableObject
    {
        public abstract IMovementStrategy Create(ActorContext context);
    }
}
