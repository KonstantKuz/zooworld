using Core.Actors.Configs;
using UnityEngine;

namespace Core.Actors.Abstract
{
    public interface IActorLifecycle
    {
        int ActiveCount { get; }
        IActor Spawn(ActorConfig config, Vector3 position);
        void Kill(IActor victim, IActor killer);
    }
}
