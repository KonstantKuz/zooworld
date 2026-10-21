using Core.Actors.Modules.Movement.Abstract;
using Core.Actors.Modules;
using Game.Actors.Modules.Movement;
using UnityEngine;

namespace Game.Actors.Modules.Movement.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Movement/Linear", fileName = "LinearMovement")]
    public sealed class LinearMovementConfig : PhysicalMovementConfig
    {
        [SerializeField] private float speed = 3f;
        [Tooltip("How fast the actor gets up to speed. The floor friction takes about 4 of it.")]
        [SerializeField] private float acceleration = 30f;

        public override IMovementStrategy Create(ActorContext context)
        {
            Rigidbody body = context.Get<Rigidbody>();
            return new LinearMovement(body, CreateFacing(body), speed, acceleration);
        }
    }
}
