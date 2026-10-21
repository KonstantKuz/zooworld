using System.Collections.Generic;
using UnityEngine;

namespace Core.Actors.Modules.Behaviour.Abstract
{
    public abstract class BehaviourConfig : ScriptableObject
    {
        public abstract IBehaviourNode Create(in ActorContext context);

        protected static List<IBehaviourNode> CreateChildren(IReadOnlyList<BehaviourConfig> configs,
            in ActorContext context)
        {
            List<IBehaviourNode> nodes = new List<IBehaviourNode>(configs.Count);
            for (int i = 0; i < configs.Count; i++)
            {
                nodes.Add(configs[i].Create(context));
            }

            return nodes;
        }
    }
}
