using Core.Actors.Modules.Movement.Abstract;
using Core.Actors.Modules;
using Game.Actors.Modules.Movement;
using UnityEngine;

namespace Game.Actors.Modules.Movement.Configs
{
    [CreateAssetMenu(menuName = "ZooWorld/Movement/Jump", fileName = "JumpMovement")]
    public sealed class JumpMovementConfig : PhysicalMovementConfig
    {
        [SerializeField] private float interval = 1.2f;
        [SerializeField] private float distance = 3f;
        [SerializeField] private float height = 0.8f;
        [SerializeField] private float groundProbe = 0.6f;
        [SerializeField] private float groundBrake = 6f;

        public override IMovementStrategy Create(ActorContext context)
        {
            Rigidbody body = context.Get<Rigidbody>();
            float gravity = Physics.gravity.magnitude;
            float verticalSpeed = Mathf.Sqrt(2f * gravity * height);
            float horizontalSpeed = distance / (2f * verticalSpeed / gravity);
            float initialTimer = Random.value * interval;

            return new JumpMovement(body, CreateFacing(body), interval, groundProbe, groundBrake,
                verticalSpeed, horizontalSpeed, initialTimer);
        }
    }
}
