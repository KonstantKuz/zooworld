using System.Collections;
using System.Collections.Generic;
using Core.Pooling.Abstract;
using Game.Level;
using Game.Level.Configs;
using Game.Navigation;
using Game.Navigation.Configs;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests
{
    public sealed class NavigationTests
    {
        private readonly List<GameObject> spawned = new();
        private NavMeshNavigationService navigation;

        [SetUp]
        public void SetUp()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(40f, 1f, 40f);
            spawned.Add(floor);

            navigation = new NavMeshNavigationService(new FakePlayfield(new Vector3(3f, 0f, 3f)), new NavigationSettings());
        }

        [TearDown]
        public void TearDown()
        {
            navigation.Dispose();
            foreach (GameObject go in spawned)
            {
                Object.Destroy(go);
            }

            spawned.Clear();
        }

        [UnityTest]
        public IEnumerator OnOpenGround_ThePathIsAStraightLine()
        {
            navigation.Rebuild();
            yield return null;

            List<Vector3> corners = new();
            bool found = navigation.TryBuildPath(new Vector3(-5f, 0f, 0f), new Vector3(5f, 0f, 0f), corners);

            Assert.IsTrue(found);
            Assert.AreEqual(2, corners.Count);
        }

        [UnityTest]
        public IEnumerator AWallInTheWay_IsWalkedAround()
        {
            AddWall(new Vector3(0f, 0.75f, 0f), new Vector3(1f, 1.5f, 10f));
            navigation.Rebuild();
            yield return null;

            List<Vector3> corners = new();
            bool found = navigation.TryBuildPath(new Vector3(-5f, 0f, 0f), new Vector3(5f, 0f, 0f), corners);

            Assert.IsTrue(found);
            Assert.Greater(corners.Count, 2, "the path must bend around the wall");
            Assert.IsTrue(corners.Exists(corner => Mathf.Abs(corner.z) > 5f), "and clear the wall's end");
        }

        [UnityTest]
        public IEnumerator WhereThereIsNoGround_NoPathExists()
        {
            navigation.Rebuild();
            yield return null;

            List<Vector3> corners = new();

            Assert.IsFalse(navigation.TryBuildPath(new Vector3(-5f, 0f, 0f), new Vector3(200f, 0f, 200f), corners));
            Assert.IsEmpty(corners);
        }

        [UnityTest]
        public IEnumerator RandomPoint_LiesOnTheWalkableGround()
        {
            navigation.Rebuild();
            yield return null;

            Assert.IsTrue(navigation.TryGetRandomPoint(out Vector3 point));
            Assert.AreEqual(0f, point.y, 0.2f);
            Assert.Less(Mathf.Abs(point.x), 20f);
        }

        [UnityTest]
        public IEnumerator AGeneratedLevel_StaysFullyConnected()
        {
            for (int run = 0; run < 10; run++)
            {
                WallPool pool = new(spawned);
                new LevelGenerationService(NewPrefab(), new LevelSettings(), pool, navigation).Initialize();
                yield return null;

                Vector3[] spots =
                {
                    new(-15f, 0f, -15f), new(15f, 0f, -15f), new(-15f, 0f, 15f), new(15f, 0f, 15f), new(0f, 0f, 0f)
                };
                List<Vector3> corners = new();
                foreach (Vector3 from in spots)
                {
                    foreach (Vector3 to in spots)
                    {
                        Assert.IsTrue(navigation.TryBuildPath(from, to, corners), $"no path {from} -> {to} (run {run})");
                        Assert.Less(Vector3.Distance(corners[corners.Count - 1], to), 2.5f,
                            $"path {from} -> {to} ends short (run {run})");
                    }
                }

                foreach (Obstacle wall in pool.Walls)
                {
                    Object.Destroy(wall.gameObject);
                }

                yield return null;
            }
        }

        private void AddWall(Vector3 position, Vector3 size)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = position;
            wall.transform.localScale = size;
            spawned.Add(wall);
        }

        private Obstacle NewPrefab()
        {
            GameObject go = new("ObstaclePrefab");
            spawned.Add(go);
            return go.AddComponent<Obstacle>();
        }

        private sealed class WallPool : IPoolService
        {
            private readonly List<GameObject> spawned;

            public WallPool(List<GameObject> spawned)
            {
                this.spawned = spawned;
            }

            public List<Obstacle> Walls { get; } = new();

            public void Prewarm<T>(T prefab, int count) where T : Component
            {
            }

            public T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetPositionAndRotation(position, rotation);
                spawned.Add(cube);
                T instance = cube.AddComponent<T>();
                Walls.Add(instance as Obstacle);
                return instance;
            }

            public void Release<T>(T instance) where T : Component
            {
            }
        }
    }
}
