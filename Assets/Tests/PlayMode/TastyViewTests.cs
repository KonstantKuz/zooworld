using System.Collections;
using System.Reflection;
using Core.Actors;
using Core.Pooling.Abstract;
using NUnit.Framework;
using UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Tests
{
    /// <summary>Brief: each time a predator eats another animal, the label "Tasty!" appears under it.</summary>
    public sealed class TastyViewTests
    {
        private FakeEvents events;
        private GameObject root;
        private GameObject prefab;
        private GameObject cameraObject;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            events = new FakeEvents();
            cameraObject = new GameObject("Camera", typeof(Camera));
            prefab = new GameObject("Label", typeof(Text), typeof(ActorLabel));
            prefab.SetActive(false);

            root = new GameObject("Tasty", typeof(RectTransform));
            root.SetActive(false);
            TastyView view = root.AddComponent<TastyView>();
            typeof(TastyView).GetField("labelPrefab", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(view, prefab.GetComponent<ActorLabel>());
            typeof(TastyView).GetMethod("Construct", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(view, new object[] { events, new InstancePool(), cameraObject.GetComponent<Camera>() });
            root.SetActive(true);
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(root);
            Object.Destroy(prefab);
            Object.Destroy(cameraObject);
        }

        private int ShownLabels()
        {
            return root.GetComponentsInChildren<ActorLabel>().Length;
        }

        [UnityTest]
        public IEnumerator WhenAPredatorEats_ATastyLabelAppears()
        {
            FakeActor predator = FakeActor.Predator(1).With(new FakeBody(Vector3.zero));

            events.Subject.OnNext(new ActorDeath(FakeActor.Prey(2), predator));
            yield return null;

            Assert.AreEqual(1, ShownLabels());
            Assert.AreEqual("Tasty!", root.GetComponentInChildren<Text>().text);
        }

        [UnityTest]
        public IEnumerator EachMealGetsItsOwnLabel()
        {
            FakeActor predator = FakeActor.Predator(1).With(new FakeBody(Vector3.zero));

            events.Subject.OnNext(new ActorDeath(FakeActor.Prey(2), predator));
            events.Subject.OnNext(new ActorDeath(FakeActor.Prey(3), predator));
            yield return null;

            Assert.AreEqual(2, ShownLabels());
        }

        [UnityTest]
        public IEnumerator DeathsThatAreNotMealsOfAPredator_ShowNothing()
        {
            events.Subject.OnNext(new ActorDeath(FakeActor.Predator(1), null));
            events.Subject.OnNext(new ActorDeath(FakeActor.Prey(2), FakeActor.Prey(3)));
            yield return null;

            Assert.AreEqual(0, ShownLabels());
        }

        [UnityTest]
        public IEnumerator TheLabelDisappearsAfterItsDuration()
        {
            events.Subject.OnNext(new ActorDeath(FakeActor.Prey(2), FakeActor.Predator(1)));
            yield return new WaitForSeconds(prefab.GetComponent<ActorLabel>().Duration + 0.3f);

            Assert.AreEqual(0, ShownLabels());
        }

        private sealed class InstancePool : IPoolService
        {
            public void Prewarm<T>(T prefab, int count) where T : Component
            {
            }

            public T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
            {
                T instance = Object.Instantiate(prefab, parent, false);
                instance.gameObject.SetActive(true);
                return instance;
            }

            public void Release<T>(T instance) where T : Component
            {
                Object.Destroy(instance.gameObject);
            }
        }
    }
}
