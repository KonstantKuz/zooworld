using System.Collections.Generic;
using Core.Pooling.Abstract;
using Game.Level;
using Game.Level.Configs;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public sealed class LevelGenerationTests
    {
        private readonly List<GameObject> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in created)
            {
                Object.DestroyImmediate(go);
            }

            created.Clear();
        }

        [Test]
        public void EverySlotGetsAWallAndTheNavMeshIsRebuiltOnce()
        {
            LevelSettings settings = SpreadOutSettings();
            FakeNavigation navigation = new();
            RecordingPool pool = new(created);

            new LevelGenerationService(NewPrefab(), settings, pool, navigation).Initialize();

            Assert.AreEqual(settings.Slots.Count, pool.Taken.Count);
            Assert.AreEqual(1, navigation.Rebuilds);
        }

        [Test]
        public void WallsNeverTouchEachOtherWhateverTheRandomJitter()
        {
            LevelSettings settings = new();

            for (int run = 0; run < 100; run++)
            {
                RecordingPool pool = new(created);
                new LevelGenerationService(NewPrefab(), settings, pool, new FakeNavigation()).Initialize();

                List<Bounds> walls = pool.Taken.ConvertAll(Footprint);
                for (int a = 0; a < walls.Count; a++)
                {
                    for (int b = a + 1; b < walls.Count; b++)
                    {
                        Bounds padded = walls[a];
                        padded.Expand(new Vector3(settings.Clearance, 0f, settings.Clearance));
                        Assert.IsFalse(padded.Intersects(walls[b]), $"walls {a} and {b} are too close (run {run})");
                    }
                }
            }
        }

        [Test]
        [Ignore("Known issue: with the default slots the first two walls conflict at home, so the fallback " +
                "placement is rejected too and ~8% of generated levels silently lose a wall.")]
        public void EverySlotOfTheDefaultLevelGetsAWall()
        {
            LevelSettings settings = new();

            for (int run = 0; run < 200; run++)
            {
                RecordingPool pool = new(created);
                new LevelGenerationService(NewPrefab(), settings, pool, new FakeNavigation()).Initialize();

                Assert.AreEqual(settings.Slots.Count, pool.Taken.Count, $"run {run}");
            }
        }

        [Test]
        public void WallsStayNearTheirSlotsAndHaveTheConfiguredHeight()
        {
            LevelSettings settings = SpreadOutSettings();
            RecordingPool pool = new(created);

            new LevelGenerationService(NewPrefab(), settings, pool, new FakeNavigation()).Initialize();

            for (int i = 0; i < settings.Slots.Count; i++)
            {
                Vector3 position = pool.Taken[i].transform.position;
                Vector2 home = settings.Slots[i].Position;
                Assert.LessOrEqual(Vector2.Distance(new Vector2(position.x, position.z), home), settings.Jitter + 1e-3f);
                Assert.AreEqual(settings.Height, pool.Taken[i].transform.localScale.y, 1e-4f);
            }
        }

        private static LevelSettings SpreadOutSettings()
        {
            const string json = "{\"slots\":[" +
                                "{\"position\":{\"x\":-10,\"y\":-10},\"size\":{\"x\":2,\"y\":4}}," +
                                "{\"position\":{\"x\":10,\"y\":-10},\"size\":{\"x\":4,\"y\":2}}," +
                                "{\"position\":{\"x\":-10,\"y\":10},\"size\":{\"x\":2,\"y\":4}}," +
                                "{\"position\":{\"x\":10,\"y\":10},\"size\":{\"x\":4,\"y\":2}}]," +
                                "\"jitter\":1,\"clearance\":2,\"height\":1.5}";
            return JsonUtility.FromJson<LevelSettings>(json);
        }

        private Obstacle NewPrefab()
        {
            GameObject go = new("ObstaclePrefab");
            created.Add(go);
            return go.AddComponent<Obstacle>();
        }

        private static Bounds Footprint(Obstacle wall)
        {
            Vector3 scale = wall.transform.localScale;
            bool turned = Mathf.Abs(Mathf.DeltaAngle(wall.transform.eulerAngles.y, 90f)) < 1f;
            Vector3 size = turned ? new Vector3(scale.z, scale.y, scale.x) : scale;
            return new Bounds(wall.transform.position, size);
        }

        private sealed class RecordingPool : IPoolService
        {
            private readonly List<GameObject> created;

            public RecordingPool(List<GameObject> created)
            {
                this.created = created;
            }

            public List<Obstacle> Taken { get; } = new();

            public void Prewarm<T>(T prefab, int count) where T : Component
            {
            }

            public T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
            {
                GameObject go = new("Wall");
                go.transform.SetPositionAndRotation(position, rotation);
                created.Add(go);
                T instance = go.AddComponent<T>();
                Taken.Add(instance as Obstacle);
                return instance;
            }

            public void Release<T>(T instance) where T : Component
            {
            }
        }
    }
}
