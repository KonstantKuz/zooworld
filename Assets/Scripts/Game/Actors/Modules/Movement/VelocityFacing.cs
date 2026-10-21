using UnityEngine;

namespace Game.Actors.Modules.Movement
{
    public sealed class VelocityFacing
    {
        private const float MinSpeedSqr = 0.09f;

        private readonly Rigidbody body;
        private readonly float turnSpeed;

        public VelocityFacing(Rigidbody body, float turnSpeed)
        {
            this.body = body;
            this.turnSpeed = turnSpeed;
        }

        public void Tick(float deltaTime)
        {
            Vector3 velocity = body.linearVelocity;
            velocity.y = 0f;
            if (velocity.sqrMagnitude < MinSpeedSqr)
            {
                return;
            }

            Quaternion target = Quaternion.LookRotation(velocity);
            body.MoveRotation(Quaternion.Slerp(body.rotation, target, turnSpeed * deltaTime));
        }
    }
}
