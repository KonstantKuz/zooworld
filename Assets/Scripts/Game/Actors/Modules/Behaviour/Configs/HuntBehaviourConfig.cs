using Core.Actors.Modules.Behaviour.Abstract;
using Core.Actors.Modules.Movement.Abstract;
using Core.Actors.Modules;
using Game.Actors.Modules.Behaviour;
using Game.Actors.Modules.Movement;
using UnityEngine;

namespace Game.Actors.Modules.Behaviour.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Behaviour/Hunt", fileName = "Hunt")]
    public sealed class HuntBehaviourConfig : BehaviourConfig
    {
        [Tooltip("The gait: how the hunter moves, both when chasing and when searching.")]
        [SerializeField] private MovementConfig movement;
        [Tooltip("Seconds without eating before the animal gets hungry.")]
        [SerializeField, Min(0f)] private float hungerSeconds = 8f;
        [Tooltip("Seconds a hungry animal may go without a meal before it starves.")]
        [SerializeField, Min(0f)] private float starveSeconds = 5f;
        [Tooltip("The color the hunter turns towards while hungry.")]
        [SerializeField] private Color tint = new(1f, 0.8f, 0.1f);
        [Tooltip("Prey closer than this gets chased.")]
        [SerializeField, Min(0f)] private float radius = 6f;

        public override IBehaviourNode Create(in ActorContext context)
        {
            ChaseRoute chaseRoute = context.Create<ChaseRoute>();
            WanderRoute searchRoute = context.Create<WanderRoute>();
            return context.Create<HuntNode>(context.Owner, context.Lifetime, movement, chaseRoute, searchRoute,
                radius, hungerSeconds, starveSeconds, tint);
        }
    }
}
