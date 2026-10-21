using Core.Actors.Configs;
using UnityEngine;

namespace Core.Actors.Abstract
{
    public interface IActorFactory
    {
        int ActiveCount { get; }

        IActor Create(ActorConfig config, Vector3 position);

        void Destroy(IActor actor);
    }
}
