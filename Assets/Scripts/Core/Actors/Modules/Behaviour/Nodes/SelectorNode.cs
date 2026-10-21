using System.Collections.Generic;
using Core.Actors.Modules.Behaviour.Abstract;

namespace Core.Actors.Modules.Behaviour.Nodes
{
    public sealed class SelectorNode : IBehaviourNode
    {
        private const int None = -1;

        private readonly List<IBehaviourNode> children;
        private int active = None;

        public SelectorNode(List<IBehaviourNode> children)
        {
            this.children = children;
        }

        public void Initialize()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Initialize();
            }
        }

        public BehaviourStatus Tick(float deltaTime)
        {
            for (int i = 0; i < children.Count; i++)
            {
                BehaviourStatus status = children[i].Tick(deltaTime);
                if (status == BehaviourStatus.Failure)
                {
                    continue;
                }

                if (active != None && active != i)
                {
                    children[active].Reset();
                }

                active = status == BehaviourStatus.Running ? i : None;
                return status;
            }

            Reset();
            return BehaviourStatus.Failure;
        }

        public void DrawGizmos()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].DrawGizmos();
            }
        }

        public void Reset()
        {
            if (active != None)
            {
                children[active].Reset();
                active = None;
            }
        }
    }
}
