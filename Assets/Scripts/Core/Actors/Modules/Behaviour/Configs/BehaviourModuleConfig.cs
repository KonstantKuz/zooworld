using Core.Actors.Modules.Abstract;
using Core.Actors.Modules.Behaviour.Abstract;
using UnityEngine;

namespace Core.Actors.Modules.Behaviour.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Modules/Behaviour", fileName = "BehaviourModule")]
    public sealed class BehaviourModuleConfig : ActorModuleConfig
    {
        [SerializeField] private BehaviourConfig root;

        public override IActorModule Create(in ActorContext context)
        {
            return new BehaviourModule(root.Create(context));
        }
    }
}
