using UnityEngine;

namespace Core.Actors.Modules.Abstract
{
    public abstract class ActorModuleConfig : ScriptableObject
    {
        public abstract IActorModule Create(in ActorContext context);
    }
}
