using System.Collections.Generic;
using Core.Actors.Modules.Behaviour.Abstract;
using UnityEngine;

namespace Core.Actors.Modules.Behaviour.Nodes.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Behaviour/Sequence", fileName = "Sequence")]
    public sealed class SequenceBehaviourConfig : BehaviourConfig
    {
        [SerializeField] private List<BehaviourConfig> children = new();

        public override IBehaviourNode Create(in ActorContext context)
        {
            return new SequenceNode(CreateChildren(children, context));
        }
    }
}
