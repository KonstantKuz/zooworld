using System;
using System.Collections;
using Core.Pooling;
using Core.Pooling.Configs;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Zenject;
using Object = UnityEngine.Object;

namespace Tests
{
    public sealed class PoolServiceTests
    {
        private sealed class Probe : MonoBehaviour
        {
        }

        private Probe prefab;
        private PoolService pools;

        [SetUp]
        public void SetUp()
        {
            prefab = new GameObject("Probe").AddComponent<Probe>();
            prefab.gameObject.SetActive(false);
            pools = new PoolService(new DiContainer(), new PoolSettings());
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Probe probe in Object.FindObjectsByType<Probe>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.Destroy(probe.gameObject);
            }

            GameObject root = GameObject.Find("Pools");
            if (root != null)
            {
                Object.Destroy(root);
            }
        }

        [Test]
        public void Get_GivesAnActiveInstanceAtTheRequestedPlace()
        {
            Probe probe = pools.Get(prefab, new Vector3(1f, 2f, 3f), Quaternion.identity);

            Assert.IsTrue(probe.gameObject.activeSelf);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), probe.transform.position);
        }

        [Test]
        public void Release_ParksTheInstanceAndTheNextGetReusesIt()
        {
            Probe first = pools.Get(prefab, Vector3.zero, Quaternion.identity);

            pools.Release(first);
            Assert.IsFalse(first.gameObject.activeSelf);

            Probe second = pools.Get(prefab, Vector3.one, Quaternion.identity);
            Assert.AreSame(first, second);
            Assert.IsTrue(second.gameObject.activeSelf);
            Assert.AreEqual(Vector3.one, second.transform.position);
        }

        [Test]
        public void Prewarm_CreatesInstancesAheadSoGetDoesNotAllocateNewOnes()
        {
            pools.Prewarm(prefab, 3);
            int before = CountProbes();

            pools.Get(prefab, Vector3.zero, Quaternion.identity);
            pools.Get(prefab, Vector3.zero, Quaternion.identity);
            pools.Get(prefab, Vector3.zero, Quaternion.identity);

            Assert.AreEqual(before, CountProbes());
        }

        [Test]
        public void ReleasingTheSameInstanceTwice_Throws()
        {
            Probe probe = pools.Get(prefab, Vector3.zero, Quaternion.identity);
            pools.Release(probe);

            Assert.Throws<InvalidOperationException>(() => pools.Release(probe));
        }

        [Test]
        public void ReleasingSomethingThatCameNotFromThePool_Throws()
        {
            Probe stranger = new GameObject("Stranger").AddComponent<Probe>();

            Assert.Throws<InvalidOperationException>(() => pools.Release(stranger));
        }

        [UnityTest]
        public IEnumerator Release_DestroysInstancesBeyondTheIdleLimit()
        {
            Probe[] taken = new Probe[45];
            for (int i = 0; i < taken.Length; i++)
            {
                taken[i] = pools.Get(prefab, Vector3.zero, Quaternion.identity);
            }

            foreach (Probe probe in taken)
            {
                pools.Release(probe);
            }

            yield return null;

            int alive = 0;
            foreach (Probe probe in taken)
            {
                if (probe != null)
                {
                    alive++;
                }
            }

            Assert.AreEqual(new PoolSettings().MaxIdle, alive);
        }

        private static int CountProbes()
        {
            return Object.FindObjectsByType<Probe>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        }
    }
}
