using UnityEngine;

namespace Core.Actors.Modules.Abstract
{
    public interface IPushable : IActorModule
    {
        void Push(Vector3 velocityChange);
    }
}
