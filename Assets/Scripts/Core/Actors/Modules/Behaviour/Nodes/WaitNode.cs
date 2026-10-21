using Core.Actors.Abstract;
using Core.Actors.Modules.Abstract;
using Core.Actors.Modules.Behaviour.Abstract;

namespace Core.Actors.Modules.Behaviour.Nodes
{
    public sealed class WaitNode : IBehaviourNode
    {
        private readonly IActor owner;
        private readonly float seconds;
        private IMovement movement;
        private float elapsed;

        public WaitNode(IActor owner, float seconds)
        {
            this.owner = owner;
            this.seconds = seconds;
        }

        public void Initialize()
        {
            owner.TryGetModule(out movement);
        }

        public BehaviourStatus Tick(float deltaTime)
        {
            movement?.Stop(deltaTime);
            elapsed += deltaTime;
            if (elapsed < seconds)
            {
                return BehaviourStatus.Running;
            }

            elapsed = 0f;
            return BehaviourStatus.Success;
        }

        public void Reset()
        {
            elapsed = 0f;
        }
    }
}
