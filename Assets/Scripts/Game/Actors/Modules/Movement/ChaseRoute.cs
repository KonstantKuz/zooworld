using Core.Actors.Modules.Movement.Abstract;
using Core.Navigation.Abstract;
using UnityEngine;

namespace Game.Actors.Modules.Movement
{
    public sealed class ChaseRoute : IRoute
    {
        private const float ReplanInterval = 0.25f;

        private readonly INavigationService navigation;
        private readonly PathFollower follower = new();
        private Vector3 target;
        private float sinceReplan = ReplanInterval;

        public ChaseRoute(INavigationService navigation)
        {
            this.navigation = navigation;
        }

        public void SetTarget(Vector3 target)
        {
            this.target = target;
        }

        public void Reset()
        {
            sinceReplan = ReplanInterval;
        }

        public Vector3 NextDirection(Vector3 position, float deltaTime)
        {
            sinceReplan += deltaTime;
            if (sinceReplan >= ReplanInterval || follower.IsFinished)
            {
                Replan(position);
            }

            follower.SkipReached(position);
            return follower.Direction(position);
        }

        private void Replan(Vector3 position)
        {
            sinceReplan = 0f;
            follower.Restart();
            if (!navigation.TryBuildPath(position, target, follower.Corners))
            {
                follower.Corners.Clear();
                follower.Corners.Add(target);
            }
        }
    }
}
