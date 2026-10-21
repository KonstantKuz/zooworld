using Core.Actors.Modules.Behaviour.Abstract;
using Core.Actors.Modules.Behaviour.Nodes;
using Core.Actors.Modules.Movement.Abstract;
using Core.Actors.Modules;
using Game.Actors.Modules.Movement;
using UnityEngine;

namespace Game.Actors.Modules.Behaviour.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Behaviour/Move", fileName = "Move")]
    public sealed class MoveBehaviourConfig : BehaviourConfig
    {
        [Tooltip("The gait: how the actor moves.")]
        [SerializeField] private MovementConfig movement;
        [Tooltip("0 moves forever.")]
        [SerializeField, Min(0f)] private float duration;

        public override IBehaviourNode Create(in ActorContext context)
        {
            WanderRoute route = context.Create<WanderRoute>();
            return new MoveNode(context.Owner, movement, route, duration);
        }
    }
}
