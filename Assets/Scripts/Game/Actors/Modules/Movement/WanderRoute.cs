using Core.Actors.Modules.Movement.Abstract;
using Core.Navigation.Abstract;
using UnityEngine;

namespace Game.Actors.Modules.Movement
{
    public sealed class WanderRoute : IRoute
    {
        private const float MaxRouteTime = 8f;
        private const float ReturnInset = 3f;

        private readonly INavigationService navigation;
        private readonly IPlayfieldService playfield;
        private readonly PathFollower follower = new();
        private float age;
        private bool returning;

        public WanderRoute(INavigationService navigation, IPlayfieldService playfield)
        {
            this.navigation = navigation;
            this.playfield = playfield;
        }

        public Vector3 NextDirection(Vector3 position, float deltaTime)
        {
            age += deltaTime;
            bool outside = !playfield.Contains(position);
            follower.SkipReached(position);

            if (follower.IsFinished || age > MaxRouteTime || (outside && !returning))
            {
                returning = outside;
                Plan(position);
                follower.SkipReached(position);
            }

            return follower.Direction(position);
        }

        private void Plan(Vector3 position)
        {
            age = 0f;
            follower.Restart();
            Vector3 target = returning
                ? playfield.RandomPoint(ReturnInset)
                : navigation.TryGetRandomPoint(out Vector3 point) ? point : position;

            if (!navigation.TryBuildPath(position, target, follower.Corners))
            {
                follower.Corners.Clear();
                follower.Corners.Add(target);
            }
        }
    }
}
