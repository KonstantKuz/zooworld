using System.Collections;
using Core.Actors.Abstract;
using Game.Actors.Perception;
using Game.Actors.Traits.Abstract;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests
{
    public sealed class NearestActorFinderTests
    {
        private readonly System.Collections.Generic.List<GameObject> spawned = new();
        private NearestActorFinder finder;
        private ProbeActor seeker;

        [SetUp]
        public void SetUp()
        {
            finder = new NearestActorFinder();
            seeker = Spawn(1, new PredatorMarker(), Vector3.zero);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in spawned)
            {
                Object.Destroy(go);
            }

            spawned.Clear();
        }

        [UnityTest]
        public IEnumerator FindsTheNearestMatchingActorWithinTheRadius()
        {
            Spawn(2, new PreyMarker(), new Vector3(6f, 0f, 0f));
            ProbeActor near = Spawn(3, new PreyMarker(), new Vector3(2f, 0f, 0f));
            Spawn(4, new PreyMarker(), new Vector3(4f, 0f, 0f));
            yield return new WaitForFixedUpdate();

            bool found = finder.TryFindNearest<IPrey>(Vector3.zero, 10f, seeker, out IActor result);

            Assert.IsTrue(found);
            Assert.AreSame(near, result);
        }

        [UnityTest]
        public IEnumerator IgnoresActorsOutsideTheRadiusTheSeekerItselfAndTheWrongKind()
        {
            Spawn(2, new PreyMarker(), new Vector3(20f, 0f, 0f));
            Spawn(3, new PredatorMarker(), new Vector3(1f, 0f, 0f));
            yield return new WaitForFixedUpdate();

            bool found = finder.TryFindNearest<IPrey>(Vector3.zero, 10f, seeker, out IActor result);

            Assert.IsFalse(found);
            Assert.IsNull(result);
        }

        [UnityTest]
        public IEnumerator IgnoresActorsThatAreAlreadyDead()
        {
            ProbeActor dead = Spawn(2, new PreyMarker(), new Vector3(1f, 0f, 0f));
            dead.Health.Die();
            ProbeActor alive = Spawn(3, new PreyMarker(), new Vector3(5f, 0f, 0f));
            yield return new WaitForFixedUpdate();

            finder.TryFindNearest<IPrey>(Vector3.zero, 10f, seeker, out IActor result);

            Assert.AreSame(alive, result);
        }

        [UnityTest]
        public IEnumerator IgnoresColliderWithoutARigidbody()
        {
            GameObject scenery = GameObject.CreatePrimitive(PrimitiveType.Cube);
            scenery.transform.position = new Vector3(1f, 0f, 0f);
            spawned.Add(scenery);
            yield return new WaitForFixedUpdate();

            Assert.IsFalse(finder.TryFindNearest<IPrey>(Vector3.zero, 10f, seeker, out _));
        }

        private ProbeActor Spawn(int id, object trait, Vector3 position)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = position;
            Rigidbody body = go.AddComponent<Rigidbody>();
            body.isKinematic = true;
            ProbeActor actor = go.AddComponent<ProbeActor>();
            actor.Setup(id, trait);
            spawned.Add(go);
            return actor;
        }

        /// <summary>Scene object that looks like an actor to the physics-based perception code.</summary>
        private sealed class ProbeActor : MonoBehaviour, IActor
        {
            private object trait;

            public FakeHealth Health { get; } = new();
            public int Id { get; private set; }

            public void Setup(int id, object trait)
            {
                Id = id;
                this.trait = trait;
            }

            public bool HasTrait<T>() where T : class
            {
                return trait is T;
            }

            public bool TryGetModule<T>(out T module) where T : class
            {
                module = Health as T ?? new RecordingMovement { Position = transform.position } as T;
                return module != null;
            }
        }
    }
}
