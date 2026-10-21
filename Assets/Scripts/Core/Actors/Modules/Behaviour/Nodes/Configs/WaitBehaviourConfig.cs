using Core.Actors.Modules.Behaviour.Abstract;
using UnityEngine;

namespace Core.Actors.Modules.Behaviour.Nodes.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Behaviour/Wait", fileName = "Wait")]
    public sealed class WaitBehaviourConfig : BehaviourConfig
    {
        [SerializeField, Min(0f)] private float seconds = 1f;

        public override IBehaviourNode Create(in ActorContext context)
        {
            return new WaitNode(context.Owner, seconds);
        }
    }
}
