using Core.Actors.Abstract;
using Core.Actors.Modules.Abstract;
using Core.Actors.Modules.Behaviour.Abstract;
using Core.Actors.Modules.Movement.Abstract;

namespace Core.Actors.Modules.Behaviour.Nodes
{
    public sealed class MoveNode : IBehaviourNode
    {
        private readonly IActor owner;
        private readonly MovementConfig gait;
        private readonly IRoute route;
        private readonly float duration;
        private IMovement movement;
        private float elapsed;

        public MoveNode(IActor owner, MovementConfig gait, IRoute route, float duration)
        {
            this.owner = owner;
            this.gait = gait;
            this.route = route;
            this.duration = duration;
        }

        public void Initialize()
        {
            owner.TryGetModule(out movement);
        }

        public BehaviourStatus Tick(float deltaTime)
        {
            if (movement == null)
            {
                return BehaviourStatus.Failure;
            }

            movement.Move(gait, route, deltaTime);
            if (duration <= 0f)
            {
                return BehaviourStatus.Running;
            }

            elapsed += deltaTime;
            if (elapsed < duration)
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
