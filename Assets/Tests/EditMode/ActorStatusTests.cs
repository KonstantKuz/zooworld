using System.Collections.Generic;
using Core.Actors;
using Game.Actors.Status;
using NUnit.Framework;
using R3;

namespace Tests
{
    public sealed class ActorStatusTests
    {
        private FakeEvents events;
        private ActorStatusService service;
        private List<ActorStatusChanged> changes;

        [SetUp]
        public void SetUp()
        {
            events = new FakeEvents();
            service = new ActorStatusService(events);
            service.Initialize();
            changes = new List<ActorStatusChanged>();
            service.Changed.Subscribe(changes.Add);
        }

        [Test]
        public void SettingTheSameStatusTwice_IsReportedOnce()
        {
            FakeActor snake = FakeActor.Predator(1);

            service.Set<HungryStatus>(snake, true);
            service.Set<HungryStatus>(snake, true);

            Assert.AreEqual(1, changes.Count);
            Assert.IsTrue(changes[0].Active);
            Assert.IsInstanceOf<HungryStatus>(changes[0].Status);
        }

        [Test]
        public void ClearingAStatusThatWasNeverSet_IsIgnored()
        {
            service.Set<ScaredStatus>(FakeActor.Prey(1), false);

            Assert.IsEmpty(changes);
        }

        [Test]
        public void StatusesOfOneActorAreIndependentFromOtherActors()
        {
            FakeActor first = FakeActor.Prey(1);
            FakeActor second = FakeActor.Prey(2);

            service.Set<ScaredStatus>(first, true);
            service.Set<ScaredStatus>(second, true);
            service.Set<ScaredStatus>(first, false);

            Assert.AreEqual(3, changes.Count);
            Assert.AreSame(second, changes[1].Actor);
            Assert.IsFalse(changes[2].Active);
            Assert.AreSame(first, changes[2].Actor);
        }

        [Test]
        public void WhenAnActorDies_ItsActiveStatusesAreCleared()
        {
            FakeActor frog = FakeActor.Prey(1);
            service.Set<ScaredStatus>(frog, true);
            changes.Clear();

            events.Subject.OnNext(new ActorDeath(frog, FakeActor.Predator(2)));

            Assert.AreEqual(1, changes.Count);
            Assert.IsFalse(changes[0].Active);
            Assert.AreSame(frog, changes[0].Actor);
        }
    }
}
