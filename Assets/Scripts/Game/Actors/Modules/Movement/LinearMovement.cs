using Core.Actors.Modules.Movement.Abstract;
using UnityEngine;

namespace Game.Actors.Modules.Movement
{
    public sealed class LinearMovement : IMovementStrategy
    {
        private readonly Rigidbody body;
        private readonly VelocityFacing facing;
        private readonly float speed;
        private readonly float acceleration;

        public LinearMovement(Rigidbody body, VelocityFacing facing, float speed, float acceleration)
        {
            this.body = body;
            this.facing = facing;
            this.speed = speed;
            this.acceleration = acceleration;
        }

        public void Tick(IRoute route, float deltaTime)
        {
            facing.Tick(deltaTime);

            Accelerate(route.NextDirection(body.position, deltaTime) * speed, deltaTime);
        }

        public void Stop(float deltaTime)
        {
            facing.Tick(deltaTime);
            Accelerate(Vector3.zero, deltaTime);
        }

        private void Accelerate(Vector3 desired, float deltaTime)
        {
            Vector3 velocity = body.linearVelocity;
            Vector3 change = desired - new Vector3(velocity.x, 0f, velocity.z);
            body.AddForce(Vector3.ClampMagnitude(change / deltaTime, acceleration), ForceMode.Acceleration);
        }
    }
}
