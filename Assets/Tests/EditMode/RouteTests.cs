using System.Collections.Generic;
using Game.Actors.Modules.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public sealed class RouteTests
    {
        private static void AssertDirection(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, 1e-4f);
            Assert.AreEqual(expected.y, actual.y, 1e-4f);
            Assert.AreEqual(expected.z, actual.z, 1e-4f);
        }

        [Test]
        public void PathFollower_HeadsForTheCurrentCornerOnTheGroundPlane()
        {
            PathFollower follower = new();
            follower.Corners.Add(new Vector3(10f, 5f, 0f));

            AssertDirection(Vector3.right, follower.Direction(Vector3.zero));
        }

        [Test]
        public void PathFollower_SkipsCornersThatAreReached()
        {
            PathFollower follower = new();
            follower.Corners.AddRange(new[] { new Vector3(0.3f, 0f, 0f), new Vector3(0f, 0f, 10f) });

            follower.SkipReached(Vector3.zero);

            AssertDirection(Vector3.forward, follower.Direction(Vector3.zero));
        }

        [Test]
        public void PathFollower_SkipsACornerThatWasOvershot()
        {
            PathFollower follower = new();
            follower.Corners.AddRange(new[] { Vector3.zero, new Vector3(5f, 0f, 0f), new Vector3(5f, 0f, 10f) });
            follower.SkipReached(Vector3.zero);

            Vector3 position = new(8f, 0f, 3f);
            follower.SkipReached(position);

            AssertDirection(new Vector3(-3f, 0f, 7f).normalized, follower.Direction(position));
        }

        [Test]
        public void PathFollower_IsFinishedAfterTheLastCornerAndGivesNoDirection()
        {
            PathFollower follower = new();
            follower.Corners.Add(Vector3.zero);

            follower.SkipReached(Vector3.zero);

            Assert.IsTrue(follower.IsFinished);
            AssertDirection(Vector3.zero, follower.Direction(Vector3.zero));
        }

        [Test]
        public void PathFollower_RestartGoesBackToTheFirstCorner()
        {
            PathFollower follower = new();
            follower.Corners.Add(Vector3.zero);
            follower.SkipReached(Vector3.zero);

            follower.Restart();

            Assert.IsFalse(follower.IsFinished);
        }

        [Test]
        public void FixedPath_WithFewerThanTwoCornersIsFinished()
        {
            FixedPathRoute route = new();
            route.Follow(new List<Vector3> { Vector3.zero });

            Assert.IsTrue(route.IsFinished);
            AssertDirection(Vector3.zero, route.NextDirection(Vector3.zero, 0.02f));
        }

        [Test]
        public void FixedPath_LeadsAlongThePathAndFinishesAtTheEnd()
        {
            FixedPathRoute route = new();
            route.Follow(new List<Vector3> { Vector3.zero, new Vector3(10f, 0f, 0f) });

            AssertDirection(Vector3.right, route.NextDirection(Vector3.zero, 0.02f));
            Assert.IsFalse(route.IsFinished);

            AssertDirection(Vector3.zero, route.NextDirection(new Vector3(9.5f, 0f, 0f), 0.02f));
            Assert.IsTrue(route.IsFinished);
        }

        [Test]
        public void FixedPath_PullsAnAnimalThatDriftedOffTheLineBackToIt()
        {
            FixedPathRoute route = new();
            route.Follow(new List<Vector3> { Vector3.zero, new Vector3(20f, 0f, 0f) });

            Vector3 direction = route.NextDirection(new Vector3(5f, 0f, 4f), 0.02f);

            Assert.Greater(direction.x, 0f, "still goes forward");
            Assert.Less(direction.z, 0f, "and back toward the line");
        }

        [Test]
        public void FixedPath_StopEndsTheRoute()
        {
            FixedPathRoute route = new();
            route.Follow(new List<Vector3> { Vector3.zero, new Vector3(10f, 0f, 0f) });

            route.Stop();

            Assert.IsTrue(route.IsFinished);
        }

        [Test]
        public void ChaseRoute_HeadsForTheTarget()
        {
            ChaseRoute route = new(new FakeNavigation());
            route.SetTarget(new Vector3(0f, 0f, 10f));

            AssertDirection(Vector3.forward, route.NextDirection(Vector3.zero, 0.02f));
        }

        [Test]
        public void ChaseRoute_FollowsAMovingTargetOnlyAfterTheReplanInterval()
        {
            ChaseRoute route = new(new FakeNavigation());
            route.SetTarget(new Vector3(0f, 0f, 10f));
            route.NextDirection(Vector3.zero, 0.02f);

            route.SetTarget(new Vector3(10f, 0f, 0f));
            AssertDirection(Vector3.forward, route.NextDirection(Vector3.zero, 0.02f));

            AssertDirection(Vector3.right, route.NextDirection(Vector3.zero, 0.5f));
        }

        [Test]
        public void ChaseRoute_WhenThereIsNoPathRunsStraightAtTheTarget()
        {
            FakeNavigation navigation = new() { Blocked = true };
            ChaseRoute route = new(navigation);
            route.SetTarget(new Vector3(0f, 0f, 10f));

            AssertDirection(Vector3.forward, route.NextDirection(Vector3.zero, 0.02f));
        }

        [Test]
        public void FleeRoute_RunsAwayFromTheDangerToTheFarthestCandidate()
        {
            FakePlayfield playfield = new(new Vector3(5f, 0f, 0f), new Vector3(-10f, 0f, 0f));
            FleeRoute route = new(new FakeNavigation(), playfield);
            route.SetDanger(new Vector3(1f, 0f, 0f));

            AssertDirection(Vector3.left, route.NextDirection(Vector3.zero, 0.02f));
        }

        [Test]
        public void FleeRoute_WithNowhereToGoStandsStill()
        {
            FleeRoute route = new(new FakeNavigation { Blocked = true }, new FakePlayfield(new Vector3(-10f, 0f, 0f)));
            route.SetDanger(new Vector3(1f, 0f, 0f));

            AssertDirection(Vector3.zero, route.NextDirection(Vector3.zero, 0.02f));
        }

        [Test]
        public void FleeRoute_IgnoresDestinationsThatAreTooClose()
        {
            FleeRoute route = new(new FakeNavigation(), new FakePlayfield(new Vector3(-2f, 0f, 0f)));
            route.SetDanger(new Vector3(1f, 0f, 0f));

            AssertDirection(Vector3.zero, route.NextDirection(Vector3.zero, 0.02f));
        }

        [Test]
        public void WanderRoute_WalksToARandomNavigablePoint()
        {
            FakeNavigation navigation = new() { RandomPoint = new Vector3(0f, 0f, 10f) };
            WanderRoute route = new(navigation, new FakePlayfield());

            AssertDirection(Vector3.forward, route.NextDirection(Vector3.zero, 0.02f));
        }

        [Test]
        public void WanderRoute_WhenOutsideThePlayfieldHeadsBackInsteadOfWandering()
        {
            FakeNavigation navigation = new() { RandomPoint = new Vector3(0f, 0f, 10f) };
            FakePlayfield playfield = new(new Vector3(-10f, 0f, 0f)) { Inside = false };
            WanderRoute route = new(navigation, playfield);

            AssertDirection(Vector3.left, route.NextDirection(new Vector3(50f, 0f, 0f), 0.02f));
        }
    }
}
