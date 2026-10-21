using Core.Actors.Modules.Abstract;
using Core.Actors.Modules;
using Game.Actors.Modules.Health;
using UnityEngine;

namespace Game.Actors.Modules.Health.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Modules/Health", fileName = "HealthModule")]
    public sealed class HealthModuleConfig : ActorModuleConfig
    {
        public override IActorModule Create(in ActorContext context)
        {
            return new HealthModule(context.Lifetime);
        }
    }
}
