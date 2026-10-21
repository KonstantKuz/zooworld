using Core.Actors.Modules.Movement.Abstract;
using Game.Actors.Modules.Movement;
using UnityEngine;

namespace Game.Actors.Modules.Movement.Configs
{
    public abstract class PhysicalMovementConfig : MovementConfig
    {
        [SerializeField, Min(0f)] private float turnSpeed = 10f;

        protected VelocityFacing CreateFacing(Rigidbody body)
        {
            return new VelocityFacing(body, turnSpeed);
        }
    }
}
