using System.Collections.Generic;
using Core.Actors.Modules.Behaviour.Abstract;

namespace Core.Actors.Modules.Behaviour.Nodes
{
    public sealed class SequenceNode : IBehaviourNode
    {
        private readonly List<IBehaviourNode> children;
        private int index;

        public SequenceNode(List<IBehaviourNode> children)
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
            while (index < children.Count)
            {
                BehaviourStatus status = children[index].Tick(deltaTime);
                if (status == BehaviourStatus.Running)
                {
                    return BehaviourStatus.Running;
                }

                if (status == BehaviourStatus.Failure)
                {
                    Reset();
                    return BehaviourStatus.Failure;
                }

                index++;
            }

            Reset();
            return BehaviourStatus.Success;
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
            if (index < children.Count)
            {
                children[index].Reset();
            }

            index = 0;
        }
    }
}
