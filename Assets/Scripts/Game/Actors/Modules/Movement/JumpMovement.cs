using Core.Actors.Modules.Movement.Abstract;
using UnityEngine;

namespace Game.Actors.Modules.Movement
{
    public sealed class JumpMovement : IMovementStrategy
    {
        private const float MaxFallSpeed = 0.5f;
        private const float MinAirTime = 0.15f;

        private readonly Rigidbody body;
        private readonly VelocityFacing facing;
        private readonly float interval;
        private readonly float groundProbe;
        private readonly float groundBrake;
        private readonly float verticalSpeed;
        private readonly float horizontalSpeed;
        private static readonly RaycastHit[] Hits = new RaycastHit[8];

        private float timer;
        private float airTime = MinAirTime;

        public JumpMovement(Rigidbody body, VelocityFacing facing, float interval,
            float groundProbe, float groundBrake, float verticalSpeed, float horizontalSpeed, float initialTimer)
        {
            this.body = body;
            this.facing = facing;
            this.interval = interval;
            this.groundProbe = groundProbe;
            this.groundBrake = groundBrake;
            this.verticalSpeed = verticalSpeed;
            this.horizontalSpeed = horizontalSpeed;
            timer = initialTimer;
        }

        public void Tick(IRoute route, float deltaTime)
        {
            facing.Tick(deltaTime);

            if (!TryBrakeOnGround(deltaTime))
            {
                return;
            }

            timer += deltaTime;
            if (timer < interval)
            {
                return;
            }

            Vector3 direction = route.NextDirection(body.position, deltaTime);
            if (direction == Vector3.zero)
            {
                return;
            }

            timer = 0f;
            airTime = 0f;
            Jump(direction);
        }

        public void Stop(float deltaTime)
        {
            facing.Tick(deltaTime);
            TryBrakeOnGround(deltaTime);
        }

        private bool TryBrakeOnGround(float deltaTime)
        {
            airTime += deltaTime;
            Vector3 velocity = body.linearVelocity;
            if (airTime < MinAirTime || velocity.y > 0.1f || velocity.y < -MaxFallSpeed || !IsOnGround())
            {
                return false;
            }

            body.AddForce(new Vector3(-velocity.x, 0f, -velocity.z) * groundBrake, ForceMode.Acceleration);
            return true;
        }

        private bool IsOnGround()
        {
            RaycastHit[] hits = Hits;
            int count = Physics.RaycastNonAlloc(body.position, Vector3.down, hits, groundProbe,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (hits[i].rigidbody != body)
                {
                    return true;
                }
            }

            return false;
        }

        private void Jump(Vector3 direction)
        {
            Vector3 launch = direction * horizontalSpeed + Vector3.up * verticalSpeed;
            body.AddForce(launch - body.linearVelocity, ForceMode.VelocityChange);
        }
    }
}
