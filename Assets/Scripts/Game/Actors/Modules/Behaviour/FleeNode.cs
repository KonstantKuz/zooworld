using Core.Actors.Abstract;
using Core.Actors.Modules.Abstract;
using Core.Actors.Modules.Behaviour.Abstract;
using Core.Actors.Modules.Behaviour;
using Core.Actors.Modules.Movement.Abstract;
using Core.Actors;
using Core.Navigation.Abstract;
using Game.Actors.Modules.Movement;
using Game.Actors.Status.Abstract;
using Game.Actors.Status;
using R3;
using UnityEngine;

namespace Game.Actors.Modules.Behaviour
{
    public sealed class FleeNode : IBehaviourNode
    {
        private readonly IActor owner;
        private readonly IActorEvents events;
        private readonly IActorStatus statuses;
        private readonly CompositeDisposable lifetime;
        private readonly MovementConfig gait;
        private readonly FleeRoute route;
        private readonly float radius;
        private readonly float seconds;
        private readonly Color tint;
        private IMovement movement;
        private float scaredUntil = -1f;
        private bool fleeing;

        public FleeNode(IActor owner, IActorEvents events, IActorStatus statuses,
            INavigationService navigation,
            IPlayfieldService playfield, CompositeDisposable lifetime,
            MovementConfig gait, float radius, float seconds, Color tint)
        {
            this.owner = owner;
            this.events = events;
            this.statuses = statuses;
            this.lifetime = lifetime;
            this.gait = gait;
            route = new FleeRoute(navigation, playfield);
            this.radius = radius;
            this.seconds = seconds;
            this.tint = tint;
        }

        public void Initialize()
        {
            owner.TryGetModule(out movement);
            events.Died
                .Where(death => death.Killer != null)
                .Subscribe(OnSomeoneEaten)
                .AddTo(lifetime);
        }

        public BehaviourStatus Tick(float deltaTime)
        {
            if (movement == null || Time.time >= scaredUntil)
            {
                Reset();
                return BehaviourStatus.Failure;
            }

            if (!fleeing)
            {
                fleeing = true;
                owner.ShowTint(tint);
                statuses.Set<ScaredStatus>(owner, true);
            }

            movement.Move(gait, route, deltaTime);
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
            if (fleeing)
            {
                owner.ClearTint();
                statuses.Set<ScaredStatus>(owner, false);
            }

            fleeing = false;
        }

        private void OnSomeoneEaten(ActorDeath death)
        {
            if (death.Victim.Id == owner.Id ||
                !owner.TryGetPosition(out Vector3 mine) ||
                !death.Victim.TryGetPosition(out Vector3 place) ||
                (mine - place).sqrMagnitude > radius * radius)
            {
                return;
            }

            route.SetDanger(place);
            scaredUntil = Time.time + seconds;
        }
    }
}
