using System.Collections;
using Core.Actors;
using Core.Actors.Modules.Behaviour;
using Game.Actors.Modules.Behaviour;
using Game.Actors.Modules.Movement;
using Game.Actors.Status;
using NUnit.Framework;
using R3;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    /// <summary>Hunger and fear are timed by <see cref="Time.time"/>, so these run in real (short) seconds.</summary>
    public sealed class PredatorPreyBehaviourTests
    {
        private const float HungerSeconds = 0.2f;
        private const float StarveSeconds = 0.3f;
        private static readonly Color HungryTint = Color.red;
        private static readonly Color ScaredTint = Color.yellow;

        private FakeEvents events;
        private FakeStatuses statuses;
        private FakeLifecycle lifecycle;
        private CompositeDisposable lifetime;
        private RecordingMovement movement;
        private FakeVisual visual;
        private FakeActor owner;
        private FakeGait gait;

        [SetUp]
        public void SetUp()
        {
            events = new FakeEvents();
            statuses = new FakeStatuses();
            lifecycle = new FakeLifecycle();
            lifetime = new CompositeDisposable();
            movement = new RecordingMovement();
            visual = new FakeVisual();
            gait = ScriptableObject.CreateInstance<FakeGait>();
        }

        [TearDown]
        public void TearDown()
        {
            lifetime.Dispose();
            Object.Destroy(gait);
        }

        private HuntNode CreateHunter()
        {
            owner = FakeActor.Predator(1).With(movement).With(visual);
            FakeNavigation navigation = new();
            HuntNode node = new(owner, events, lifecycle, statuses, lifetime, gait, new ChaseRoute(navigation),
                new WanderRoute(navigation, new FakePlayfield()), 5f, HungerSeconds, StarveSeconds,
                HungryTint);
            node.Initialize();
            return node;
        }

        private FleeNode CreateFleer(float seconds, FakePlayfield playfield = null)
        {
            owner = FakeActor.Prey(1).With(movement).With(visual);
            FleeNode node = new(owner, events, statuses, new FakeNavigation(),
                playfield ?? new FakePlayfield(new Vector3(-10f, 0f, 0f)), lifetime, gait, 8f, seconds, ScaredTint);
            node.Initialize();
            return node;
        }

        [Test]
        public void Hunt_AFreshlyFedPredatorIsNotHungry()
        {
            HuntNode hunt = CreateHunter();

            Assert.AreEqual(BehaviourStatus.Failure, hunt.Tick(0.02f));
            Assert.IsEmpty(movement.Routes);
            Assert.IsFalse(statuses.Has<HungryStatus>(owner));
        }

        [UnityTest]
        public IEnumerator Hunt_WhenHungryItSpeedsUpGetsMarkedAndSearchesForFood()
        {
            HuntNode hunt = CreateHunter();
            yield return new WaitForSeconds(HungerSeconds + 0.05f);

            Assert.AreEqual(BehaviourStatus.Running, hunt.Tick(0.02f));

            Assert.IsTrue(statuses.Has<HungryStatus>(owner));
            Assert.AreEqual(HungryTint, visual.Tint);
            Assert.IsInstanceOf<WanderRoute>(movement.Routes[0], "no prey in sight: it searches");
        }

        [UnityTest]
        public IEnumerator Hunt_EatingSomethingEndsTheHunger()
        {
            HuntNode hunt = CreateHunter();
            yield return new WaitForSeconds(HungerSeconds + 0.05f);
            hunt.Tick(0.02f);

            events.Subject.OnNext(new ActorDeath(FakeActor.Prey(2), owner));

            Assert.IsFalse(statuses.Has<HungryStatus>(owner));
            Assert.IsNull(visual.Tint);
            Assert.AreEqual(BehaviourStatus.Failure, hunt.Tick(0.02f), "it is full again");
        }

        [UnityTest]
        public IEnumerator Hunt_SomeoneElsesMealDoesNotFeedIt()
        {
            HuntNode hunt = CreateHunter();
            yield return new WaitForSeconds(HungerSeconds + 0.05f);
            hunt.Tick(0.02f);

            events.Subject.OnNext(new ActorDeath(FakeActor.Prey(2), FakeActor.Predator(3)));

            Assert.IsTrue(statuses.Has<HungryStatus>(owner));
        }

        [UnityTest]
        public IEnumerator Hunt_AStarvingPredatorDies()
        {
            HuntNode hunt = CreateHunter();
            yield return new WaitForSeconds(HungerSeconds + StarveSeconds + 0.1f);

            hunt.Tick(0.02f);

            Assert.AreEqual(1, lifecycle.Kills.Count);
            Assert.AreSame(owner, lifecycle.Kills[0].Victim);
            Assert.IsNull(lifecycle.Kills[0].Killer, "starved, not killed by anyone");
        }

        [Test]
        public void Flee_StaysCalmUntilSomethingIsEatenNearby()
        {
            FleeNode flee = CreateFleer(0.3f);

            Assert.AreEqual(BehaviourStatus.Failure, flee.Tick(0.02f));
            Assert.IsEmpty(movement.Routes);
        }

        [Test]
        public void Flee_RunsAwayWhenAMealHappensWithinTheRadius()
        {
            FleeNode flee = CreateFleer(0.3f);
            FakeActor victim = FakeActor.Prey(2).With(new FakeBody(new Vector3(3f, 0f, 0f)));

            events.Subject.OnNext(new ActorDeath(victim, FakeActor.Predator(3)));

            Assert.AreEqual(BehaviourStatus.Running, flee.Tick(0.02f));
            Assert.IsTrue(statuses.Has<ScaredStatus>(owner));
            Assert.AreEqual(ScaredTint, visual.Tint);
            Assert.AreEqual(1, movement.Routes.Count);
        }

        [Test]
        public void Flee_AFreshMealMovesTheDangerAndTheAnimalRunsFromTheNewSpot()
        {
            FleeNode flee = CreateFleer(0.3f, new FakePlayfield(new Vector3(-10f, 0f, 0f), new Vector3(10f, 0f, 0f)));
            events.Subject.OnNext(new ActorDeath(FakeActor.Prey(2).With(new FakeBody(new Vector3(3f, 0f, 0f))),
                FakeActor.Predator(3)));
            flee.Tick(0.02f);
            Assert.Less(movement.Routes[0].NextDirection(Vector3.zero, 0.02f).x, 0f, "away from the first meal");

            events.Subject.OnNext(new ActorDeath(FakeActor.Prey(4).With(new FakeBody(new Vector3(-3f, 0f, 0f))),
                FakeActor.Predator(3)));
            flee.Tick(0.02f);

            Assert.Greater(movement.Routes[1].NextDirection(Vector3.zero, 0.02f).x, 0f, "away from the newer one");
        }

        [Test]
        public void Flee_IgnoresMealsTooFarAwayAndDeathsThatAreNotMeals()
        {
            FleeNode flee = CreateFleer(0.3f);
            FakeActor far = FakeActor.Prey(2).With(new FakeBody(new Vector3(30f, 0f, 0f)));
            FakeActor near = FakeActor.Prey(3).With(new FakeBody(new Vector3(2f, 0f, 0f)));

            events.Subject.OnNext(new ActorDeath(far, FakeActor.Predator(4)));
            events.Subject.OnNext(new ActorDeath(near, null));

            Assert.AreEqual(BehaviourStatus.Failure, flee.Tick(0.02f));
        }

        [Test]
        public void Flee_ItsOwnDeathDoesNotScareItself()
        {
            FleeNode flee = CreateFleer(0.3f);

            events.Subject.OnNext(new ActorDeath(owner, FakeActor.Predator(4)));

            Assert.AreEqual(BehaviourStatus.Failure, flee.Tick(0.02f));
        }

        [UnityTest]
        public IEnumerator Flee_CalmsDownAfterTheFearRunsOut()
        {
            FleeNode flee = CreateFleer(0.2f);
            FakeActor victim = FakeActor.Prey(2).With(new FakeBody(new Vector3(3f, 0f, 0f)));
            events.Subject.OnNext(new ActorDeath(victim, FakeActor.Predator(3)));
            flee.Tick(0.02f);

            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(BehaviourStatus.Failure, flee.Tick(0.02f));
            Assert.IsFalse(statuses.Has<ScaredStatus>(owner));
            Assert.IsNull(visual.Tint);
        }
    }
}
