using Game.Actors.Interactions;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public sealed class InteractionRuleTests
    {
        [Test]
        public void PredatorEatsPrey_KillsPreyWhicheverOrderTheyMeet()
        {
            FakeLifecycle lifecycle = new();
            PredatorEatsPreyRule rule = new(lifecycle);
            FakeActor predator = FakeActor.Predator(1);
            FakeActor prey = FakeActor.Prey(2);

            Assert.IsTrue(rule.TryApply(predator, prey));
            Assert.IsTrue(rule.TryApply(prey, predator));

            Assert.AreEqual(2, lifecycle.Kills.Count);
            Assert.IsTrue(lifecycle.Kills.TrueForAll(kill => kill.Victim == prey && kill.Killer == predator));
        }

        [Test]
        public void PredatorEatsPrey_IgnoresTwoPrey()
        {
            FakeLifecycle lifecycle = new();
            PredatorEatsPreyRule rule = new(lifecycle);

            Assert.IsFalse(rule.TryApply(FakeActor.Prey(1), FakeActor.Prey(2)));
            Assert.IsEmpty(lifecycle.Kills);
        }

        [Test]
        public void PredatorDuel_OlderPredatorSurvives()
        {
            FakeLifecycle lifecycle = new();
            PredatorDuelRule rule = new(lifecycle);
            FakeActor older = FakeActor.Predator(1);
            FakeActor younger = FakeActor.Predator(2);

            rule.TryApply(younger, older);

            Assert.AreEqual(1, lifecycle.Kills.Count);
            Assert.AreSame(younger, lifecycle.Kills[0].Victim);
            Assert.AreSame(older, lifecycle.Kills[0].Killer);
        }

        [Test]
        public void PreyBounce_PushesBothAwayFromEachOtherAlongTheGround()
        {
            PreyBounceRule rule = new(3f);
            FakeBody leftBody = new(new Vector3(0f, 0f, 0f));
            FakeBody rightBody = new(new Vector3(2f, 5f, 0f));
            FakeActor left = FakeActor.Prey(1).With(leftBody);
            FakeActor right = FakeActor.Prey(2).With(rightBody);

            Assert.IsTrue(rule.TryApply(left, right));

            Assert.That(Vector3.Distance(leftBody.LastPush, new Vector3(-3f, 0f, 0f)), Is.LessThan(1e-4f));
            Assert.That(Vector3.Distance(rightBody.LastPush, new Vector3(3f, 0f, 0f)), Is.LessThan(1e-4f));
        }

        [Test]
        public void PreyBounce_StillPushesWhenPreyStandOnTheSamePoint()
        {
            PreyBounceRule rule = new(3f);
            FakeBody a = new(Vector3.zero);
            FakeBody b = new(Vector3.zero);

            rule.TryApply(FakeActor.Prey(1).With(a), FakeActor.Prey(2).With(b));

            Assert.That(a.LastPush.magnitude, Is.EqualTo(3f).Within(1e-4f));
            Assert.That(b.LastPush, Is.EqualTo(-a.LastPush));
        }
    }
}
