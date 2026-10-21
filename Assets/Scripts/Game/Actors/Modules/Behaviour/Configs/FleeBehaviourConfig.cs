using Core.Actors.Modules.Behaviour.Abstract;
using Core.Actors.Modules.Movement.Abstract;
using Core.Actors.Modules;
using Game.Actors.Modules.Behaviour;
using UnityEngine;

namespace Game.Actors.Modules.Behaviour.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Behaviour/Flee", fileName = "Flee")]
    public sealed class FleeBehaviourConfig : BehaviourConfig
    {
        [Tooltip("The gait: how the animal moves while scared (a quicker one than usual).")]
        [SerializeField] private MovementConfig movement;
        [Tooltip("The animal gets scared when someone is eaten closer than this.")]
        [SerializeField, Min(0f)] private float radius = 4f;
        [Tooltip("How long the fear lasts.")]
        [SerializeField, Min(0f)] private float seconds = 3f;
        [Tooltip("The color the animal turns towards while scared.")]
        [SerializeField] private Color tint = new(1f, 0.95f, 0.3f);

        public override IBehaviourNode Create(in ActorContext context)
        {
            return context.Create<FleeNode>(context.Owner, context.Lifetime, movement, radius, seconds, tint);
        }
    }
}
