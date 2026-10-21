using Core.Actors.Modules.Abstract;
using Core.Actors.Modules;
using Game.Actors.Modules.Collision;
using UnityEngine;

namespace Game.Actors.Modules.Collision.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Modules/Collision", fileName = "CollisionModule")]
    public sealed class CollisionModuleConfig : ActorModuleConfig
    {
        public override IActorModule Create(in ActorContext context)
        {
            return context.Create<CollisionModule>();
        }
    }
}
