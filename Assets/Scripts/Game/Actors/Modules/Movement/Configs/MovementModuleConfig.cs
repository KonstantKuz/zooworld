using Core.Actors.Modules.Abstract;
using Core.Actors.Modules;
using Game.Actors.Modules.Movement;
using UnityEngine;

namespace Game.Actors.Modules.Movement.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Modules/Movement", fileName = "MovementModule")]
    public sealed class MovementModuleConfig : ActorModuleConfig
    {
        public override IActorModule Create(in ActorContext context)
        {
            return new MovementModule(context);
        }
    }
}
