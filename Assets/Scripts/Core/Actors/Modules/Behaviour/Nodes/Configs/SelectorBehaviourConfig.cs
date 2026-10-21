using System.Collections.Generic;
using Core.Actors.Modules.Behaviour.Abstract;
using UnityEngine;

namespace Core.Actors.Modules.Behaviour.Nodes.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Behaviour/Selector", fileName = "Selector")]
    public sealed class SelectorBehaviourConfig : BehaviourConfig
    {
        [Tooltip("Ordered by priority: the first child that can run wins.")]
        [SerializeField] private List<BehaviourConfig> children = new();

        public override IBehaviourNode Create(in ActorContext context)
        {
            return new SelectorNode(CreateChildren(children, context));
        }
    }
}
