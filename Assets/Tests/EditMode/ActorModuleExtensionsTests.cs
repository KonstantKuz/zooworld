using Core.Actors.Modules.Abstract;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public sealed class ActorModuleExtensionsTests
    {
        [Test]
        public void IsAlive_IsTrueForAnActorThatHasNoHealth()
        {
            Assert.IsTrue(FakeActor.Prey(1).IsAlive());
        }

        [Test]
        public void IsAlive_FollowsTheHealthModule()
        {
            FakeHealth health = new();
            FakeActor actor = FakeActor.Prey(1).With(health);

            health.Die();

            Assert.IsFalse(actor.IsAlive());
        }

        [Test]
        public void TryGetPosition_ReadsItFromTheMovementModule()
        {
            FakeActor actor = FakeActor.Prey(1).With(new FakeBody(new Vector3(1f, 2f, 3f)));

            Assert.IsTrue(actor.TryGetPosition(out Vector3 position));
            Assert.AreEqual(new Vector3(1f, 2f, 3f), position);
        }

        [Test]
        public void TryGetPosition_FailsForAnActorThatCannotMove()
        {
            Assert.IsFalse(FakeActor.Prey(1).TryGetPosition(out _));
        }

        [Test]
        public void Tint_GoesToTheVisualModuleAndIsHarmlessWithoutOne()
        {
            FakeVisual visual = new();
            FakeActor actor = FakeActor.Prey(1).With(visual);

            actor.ShowTint(Color.red);
            Assert.AreEqual(Color.red, visual.Tint);
            actor.ClearTint();
            Assert.IsNull(visual.Tint);

            Assert.DoesNotThrow(() => FakeActor.Prey(2).ShowTint(Color.red));
        }
    }
}
