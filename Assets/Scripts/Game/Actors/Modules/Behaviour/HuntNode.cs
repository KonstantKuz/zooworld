using Core.Actors.Abstract;
using Core.Actors.Modules.Abstract;
using Core.Actors.Modules.Behaviour.Abstract;
using Core.Actors.Modules.Behaviour;
using Core.Actors.Modules.Movement.Abstract;
using Game.Actors.Modules.Movement;
using Game.Actors.Perception;
using Game.Actors.Status.Abstract;
using Game.Actors.Status;
using Game.Actors.Traits.Abstract;
using R3;
using UnityEngine;

namespace Game.Actors.Modules.Behaviour
{
    public sealed class HuntNode : IBehaviourNode
    {
        private readonly IActor owner;
        private readonly IActorEvents events;
        private readonly IActorLifecycle lifecycle;
        private readonly IActorStatus statuses;
        private readonly CompositeDisposable lifetime;
        private readonly MovementConfig gait;
        private readonly ChaseRoute chaseRoute;
        private readonly WanderRoute searchRoute;
        private readonly NearestActorFinder finder = new();
        private readonly float radius;
        private readonly float hungerSeconds;
        private readonly float starveSeconds;
        private readonly Color tint;
        private IMovement movement;
        private float lastAteTime;
        private bool hungry;
        private bool chasing;

        public HuntNode(IActor owner, IActorEvents events, IActorLifecycle lifecycle,
            IActorStatus statuses, CompositeDisposable lifetime,
            MovementConfig gait, ChaseRoute chaseRoute, WanderRoute searchRoute, float radius, float hungerSeconds,
            float starveSeconds, Color tint)
        {
            this.owner = owner;
            this.events = events;
            this.lifecycle = lifecycle;
            this.statuses = statuses;
            this.lifetime = lifetime;
            this.gait = gait;
            this.chaseRoute = chaseRoute;
            this.searchRoute = searchRoute;
            this.radius = radius;
            this.hungerSeconds = hungerSeconds;
            this.starveSeconds = starveSeconds;
            this.tint = tint;
        }

        public void Initialize()
        {
            owner.TryGetModule(out movement);
            lastAteTime = Time.time;
            events.Died
                .Where(death => death.Killer != null && death.Killer.Id == owner.Id)
                .Subscribe(_ => Feed())
                .AddTo(lifetime);
        }

        public BehaviourStatus Tick(float deltaTime)
        {
            float hungerTime = Time.time - lastAteTime;
            if (movement == null || hungerTime < hungerSeconds)
            {
                Reset();
                return BehaviourStatus.Failure;
            }

            BecomeHungry();
            if (hungerTime >= hungerSeconds + starveSeconds)
            {
                lifecycle.Kill(owner, null);
                return BehaviourStatus.Failure;
            }

            if (!owner.TryGetPosition(out Vector3 origin))
            {
                return BehaviourStatus.Failure;
            }

            if (finder.TryFindNearest<IPrey>(origin, radius, owner, out IActor prey) &&
                prey.TryGetPosition(out Vector3 target))
            {
                Chase(target, deltaTime);
            }
            else
            {
                Search(deltaTime);
            }

            return BehaviourStatus.Running;
        }

        public void DrawGizmos()
        {
            if (owner.TryGetPosition(out Vector3 position))
            {
                Gizmos.color = tint;
                Gizmos.DrawWireSphere(position, radius);
            }
        }

        public void Reset()
        {
            chasing = false;
        }

        private void Feed()
        {
            lastAteTime = Time.time;
            hungry = false;
            statuses.Set<HungryStatus>(owner, false);
            owner.ClearTint();
        }

        private void BecomeHungry()
        {
            if (hungry)
            {
                return;
            }

            hungry = true;
            owner.ShowTint(tint);
            statuses.Set<HungryStatus>(owner, true);
        }

        private void Chase(Vector3 target, float deltaTime)
        {
            if (!chasing)
            {
                chasing = true;
                chaseRoute.Reset();
            }

            chaseRoute.SetTarget(target);
            movement.Move(gait, chaseRoute, deltaTime);
        }

        private void Search(float deltaTime)
        {
            chasing = false;
            movement.Move(gait, searchRoute, deltaTime);
        }
    }
}
