using System.Collections.Generic;
using Core.Actors.Interactions.Abstract;
using Core.Actors.Interactions;
using Core.Actors;
using Game.Actors.Interactions;
using Game.Stats;
using NUnit.Framework;
using R3;
using UnityEngine;

namespace Tests
{
    /// <summary>
    /// The rules from the brief, played through the real collision pipeline wired like the game installs it:
    /// prey bounce off prey, predators eat prey and each other, only one of two duelling predators survives.
    /// </summary>
    public sealed class GameRulesTests
    {
        private const float BounceSpeed = 3f;

        private FakeFactory factory;
        private ActorLifecycleService lifecycle;
        private CollisionService collisions;
        private List<ActorDeath> deaths;
        private int nextId;

        [SetUp]
        public void SetUp()
        {
            factory = new FakeFactory();
            lifecycle = new ActorLifecycleService(factory);
            collisions = new CollisionService(new List<IInteractionRule>
            {
                new PredatorEatsPreyRule(lifecycle),
                new PredatorDuelRule(lifecycle),
                new PreyBounceRule(BounceSpeed)
            });
            deaths = new List<ActorDeath>();
            lifecycle.Died.Subscribe(deaths.Add);
            nextId = 0;
        }

        private (FakeActor Actor, FakeHealth Health, FakeBody Body) Prey(Vector3 at)
        {
            FakeHealth health = new();
            FakeBody body = new(at);
            return (FakeActor.Prey(++nextId).With(health).With(body), health, body);
        }

        private (FakeActor Actor, FakeHealth Health, FakeBody Body) Predator(Vector3 at)
        {
            FakeHealth health = new();
            FakeBody body = new(at);
            return (FakeActor.Predator(++nextId).With(health).With(body), health, body);
        }

        private void Collide(FakeActor a, FakeActor b)
        {
            collisions.Report(a, b);
            collisions.Report(b, a);
            collisions.FixedTick();
        }

        [Test]
        public void TwoPrey_FlyApartAndNobodyDies()
        {
            var left = Prey(Vector3.zero);
            var right = Prey(new Vector3(1f, 0f, 0f));

            Collide(left.Actor, right.Actor);

            Assert.IsTrue(left.Health.IsAlive);
            Assert.IsTrue(right.Health.IsAlive);
            Assert.IsEmpty(deaths);
            Assert.Less(left.Body.LastPush.x, 0f);
            Assert.Greater(right.Body.LastPush.x, 0f);
        }

        [Test]
        public void APredatorMeetingPrey_EatsItWhoeverTouchedFirst()
        {
            var predator = Predator(Vector3.zero);
            var prey = Prey(Vector3.right);

            Collide(prey.Actor, predator.Actor);

            Assert.IsFalse(prey.Health.IsAlive);
            Assert.IsTrue(predator.Health.IsAlive);
            Assert.AreEqual(1, deaths.Count);
            Assert.AreSame(predator.Actor, deaths[0].Killer);
            Assert.AreEqual(new[] { prey.Actor }, factory.Destroyed, "the victim disappears from the screen");
        }

        [Test]
        public void TwoPredators_OneSurvivesAndTheOtherDies()
        {
            var first = Predator(Vector3.zero);
            var second = Predator(Vector3.right);

            Collide(first.Actor, second.Actor);

            Assert.AreEqual(1, deaths.Count);
            Assert.AreNotEqual(first.Health.IsAlive, second.Health.IsAlive, "exactly one is left standing");
            Assert.AreSame(deaths[0].Victim == first.Actor ? second.Actor : first.Actor, deaths[0].Killer);
        }

        [Test]
        public void APredatorInAPileUp_EatsEveryoneItTouchesAndGetsTheCredit()
        {
            var predator = Predator(Vector3.zero);
            var preyA = Prey(Vector3.right);
            var preyB = Prey(Vector3.left);
            var rival = Predator(Vector3.forward);

            collisions.Report(predator.Actor, preyA.Actor);
            collisions.Report(predator.Actor, preyB.Actor);
            collisions.Report(predator.Actor, rival.Actor);
            collisions.FixedTick();

            Assert.IsFalse(preyA.Health.IsAlive);
            Assert.IsFalse(preyB.Health.IsAlive);
            Assert.IsFalse(rival.Health.IsAlive, "the older predator wins the duel");
            Assert.IsTrue(predator.Health.IsAlive);
            Assert.AreEqual(3, deaths.Count);
            Assert.IsTrue(deaths.TrueForAll(death => death.Killer == predator.Actor),
                "every meal is credited to the predator, which is what makes 'Tasty!' appear");
        }

        [Test]
        public void KillStats_CountWhatTheCollisionsProduced()
        {
            FakeEvents events = new();
            lifecycle.Died.Subscribe(events.Subject.OnNext);
            KillStatsService stats = new(events);
            stats.Initialize();

            Collide(Predator(Vector3.zero).Actor, Prey(Vector3.right).Actor);
            Collide(Predator(new Vector3(9f, 0f, 0f)).Actor, Predator(new Vector3(10f, 0f, 0f)).Actor);

            Assert.AreEqual(1, stats.DeadPrey.CurrentValue);
            Assert.AreEqual(1, stats.DeadPredators.CurrentValue);
        }
    }
}
