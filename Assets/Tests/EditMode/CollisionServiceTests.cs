using System.Collections.Generic;
using Core.Actors.Interactions.Abstract;
using Core.Actors.Interactions;
using NUnit.Framework;

namespace Tests
{
    public sealed class CollisionServiceTests
    {
        [Test]
        public void SamePairReportedByBothSides_IsResolvedOnce()
        {
            RecordingRule rule = new(true);
            CollisionService service = new(new List<IInteractionRule> { rule });
            FakeActor a = FakeActor.Prey(1);
            FakeActor b = FakeActor.Prey(2);

            service.Report(a, b);
            service.Report(b, a);
            service.FixedTick();

            Assert.AreEqual(1, rule.Calls);
        }

        [Test]
        public void ContactsAreResolvedOnlyOncePerReport()
        {
            RecordingRule rule = new(true);
            CollisionService service = new(new List<IInteractionRule> { rule });

            service.Report(FakeActor.Prey(1), FakeActor.Prey(2));
            service.FixedTick();
            service.FixedTick();

            Assert.AreEqual(1, rule.Calls);
        }

        [Test]
        public void FirstRuleThatHandlesThePairWins()
        {
            RecordingRule skipped = new(false);
            RecordingRule handled = new(true);
            RecordingRule unreached = new(true);
            CollisionService service = new(new List<IInteractionRule> { skipped, handled, unreached });

            service.Report(FakeActor.Prey(1), FakeActor.Prey(2));
            service.FixedTick();

            Assert.AreEqual(1, skipped.Calls);
            Assert.AreEqual(1, handled.Calls);
            Assert.AreEqual(0, unreached.Calls);
        }

        [Test]
        public void DeadActorTakesNoPartInAnInteraction()
        {
            RecordingRule rule = new(true);
            CollisionService service = new(new List<IInteractionRule> { rule });
            FakeHealth health = new();
            FakeActor dead = FakeActor.Prey(1).With(health);
            health.Die();

            service.Report(dead, FakeActor.Predator(2));
            service.FixedTick();

            Assert.AreEqual(0, rule.Calls);
        }
    }
}
