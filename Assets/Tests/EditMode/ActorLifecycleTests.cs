using System.Collections.Generic;
using Core.Actors;
using Game.Stats;
using NUnit.Framework;
using R3;

namespace Tests
{
    public sealed class ActorLifecycleTests
    {
        [Test]
        public void Kill_MarksVictimDeadPublishesTheDeathAndReturnsItToTheFactory()
        {
            FakeFactory factory = new();
            ActorLifecycleService lifecycle = new(factory);
            List<ActorDeath> deaths = new();
            lifecycle.Died.Subscribe(deaths.Add);
            FakeHealth health = new();
            FakeActor victim = FakeActor.Prey(1).With(health);
            FakeActor killer = FakeActor.Predator(2);

            lifecycle.Kill(victim, killer);

            Assert.IsFalse(health.IsAlive);
            Assert.AreEqual(1, deaths.Count);
            Assert.AreSame(victim, deaths[0].Victim);
            Assert.AreSame(killer, deaths[0].Killer);
            Assert.AreEqual(new[] { victim }, factory.Destroyed);
        }

        [Test]
        public void Kill_OfAnAlreadyDeadActorDoesNothing()
        {
            FakeFactory factory = new();
            ActorLifecycleService lifecycle = new(factory);
            List<ActorDeath> deaths = new();
            lifecycle.Died.Subscribe(deaths.Add);
            FakeActor victim = FakeActor.Prey(1).With(new FakeHealth());
            FakeActor killer = FakeActor.Predator(2);

            lifecycle.Kill(victim, killer);
            lifecycle.Kill(victim, killer);

            Assert.AreEqual(1, deaths.Count);
            Assert.AreEqual(1, factory.Destroyed.Count);
        }

        [Test]
        public void Kill_OfAnActorWithoutHealthIsIgnored()
        {
            FakeFactory factory = new();
            ActorLifecycleService lifecycle = new(factory);

            lifecycle.Kill(FakeActor.Prey(1), FakeActor.Predator(2));

            Assert.IsEmpty(factory.Destroyed);
        }

        [Test]
        public void KillStats_CountPreyAndPredatorsSeparately()
        {
            FakeEvents events = new();
            KillStatsService stats = new(events);
            stats.Initialize();

            events.Subject.OnNext(new ActorDeath(FakeActor.Prey(1), FakeActor.Predator(9)));
            events.Subject.OnNext(new ActorDeath(FakeActor.Prey(2), FakeActor.Predator(9)));
            events.Subject.OnNext(new ActorDeath(FakeActor.Predator(3), FakeActor.Predator(9)));

            Assert.AreEqual(2, stats.DeadPrey.CurrentValue);
            Assert.AreEqual(1, stats.DeadPredators.CurrentValue);
        }
    }
}
