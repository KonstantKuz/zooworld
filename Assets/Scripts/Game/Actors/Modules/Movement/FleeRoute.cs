using System.Collections.Generic;
using Core.Actors.Modules.Movement.Abstract;
using Core.Navigation.Abstract;
using UnityEngine;

namespace Game.Actors.Modules.Movement
{
    public sealed class FleeRoute : IRoute
    {
        private const int Candidates = 24;
        private const float EdgeInset = 1.5f;
        private const float MinRunDistance = 4f;
        private const float MinGain = 1f;

        private readonly INavigationService navigation;
        private readonly IPlayfieldService playfield;
        private readonly FixedPathRoute route = new();
        private readonly List<Vector3> candidatePath = new();
        private readonly List<Vector3> bestPath = new();
        private readonly List<Vector3> fallbackPath = new();
        private Vector3 danger;

        public FleeRoute(INavigationService navigation, IPlayfieldService playfield)
        {
            this.navigation = navigation;
            this.playfield = playfield;
        }

        public void SetDanger(Vector3 danger)
        {
            this.danger = danger;
            route.Stop();
        }

        public Vector3 NextDirection(Vector3 position, float deltaTime)
        {
            Vector3 direction = route.NextDirection(position, deltaTime);
            if (!route.IsFinished)
            {
                return direction;
            }

            Plan(position);
            return route.NextDirection(position, deltaTime);
        }

        private void Plan(Vector3 position)
        {
            float currentDistance = Vector3.Distance(position, danger);
            float bestDistance = 0f;
            bool hasBest = false;
            bool hasFallback = false;

            for (int i = 0; i < Candidates; i++)
            {
                Vector3 point = playfield.RandomPoint(EdgeInset);
                Vector3 toPoint = point - position;
                toPoint.y = 0f;
                if (toPoint.magnitude < MinRunDistance ||
                    !navigation.TryBuildPath(position, point, candidatePath) || candidatePath.Count < 2)
                {
                    continue;
                }

                float distance = Vector3.Distance(point, danger);
                if (distance > currentDistance + MinGain && distance > bestDistance)
                {
                    bestDistance = distance;
                    bestPath.Clear();
                    bestPath.AddRange(candidatePath);
                    hasBest = true;
                }
                else if (!hasFallback)
                {
                    fallbackPath.Clear();
                    fallbackPath.AddRange(candidatePath);
                    hasFallback = true;
                }
            }

            if (hasBest)
            {
                route.Follow(bestPath);
            }
            else if (hasFallback)
            {
                route.Follow(fallbackPath);
            }
            else
            {
                route.Stop();
            }
        }
    }
}
