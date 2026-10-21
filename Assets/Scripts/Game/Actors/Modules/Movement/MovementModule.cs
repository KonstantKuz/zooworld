using System.Collections.Generic;
using Core.Actors.Modules.Abstract;
using Core.Actors.Modules.Movement.Abstract;
using Core.Actors.Modules;
using UnityEngine;

namespace Game.Actors.Modules.Movement
{
    public sealed class MovementModule : IMovement, IPushable, IInitializableModule
    {
        private readonly ActorContext context;
        private readonly Rigidbody body;
        private readonly float mass;
        private readonly Dictionary<MovementConfig, IMovementStrategy> strategies = new();
        private IMovementStrategy current;

        public MovementModule(ActorContext context)
        {
            this.context = context;
            body = context.Get<Rigidbody>();
            mass = context.Config.Mass;
        }

        public Vector3 Position => body.position;

        public void Initialize()
        {
            body.mass = mass;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        public void Move(MovementConfig gait, IRoute route, float deltaTime)
        {
            if (!strategies.TryGetValue(gait, out IMovementStrategy strategy))
            {
                strategy = gait.Create(context);
                strategies[gait] = strategy;
            }

            current = strategy;
            strategy.Tick(route, deltaTime);
        }

        public void Stop(float deltaTime)
        {
            current?.Stop(deltaTime);
        }

        public void Push(Vector3 velocityChange)
        {
            body.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }
}
