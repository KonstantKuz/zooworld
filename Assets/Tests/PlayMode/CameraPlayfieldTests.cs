using Game.Navigation;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public sealed class CameraPlayfieldTests
    {
        private GameObject cameraObject;
        private CameraPlayfieldService playfield;

        [SetUp]
        public void SetUp()
        {
            cameraObject = new GameObject("TopDownCamera");
            cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 10f, 0f), Quaternion.Euler(90f, 0f, 0f));
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.aspect = 2f;
            playfield = new CameraPlayfieldService(camera);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(cameraObject);
        }

        [Test]
        public void Contains_ReflectsWhatTheCameraSeesOnTheGround()
        {
            Assert.IsTrue(playfield.Contains(new Vector3(9.5f, 0f, 4.5f)));
            Assert.IsTrue(playfield.Contains(new Vector3(-9.5f, 0f, -4.5f)));
            Assert.IsFalse(playfield.Contains(new Vector3(10.5f, 0f, 0f)));
            Assert.IsFalse(playfield.Contains(new Vector3(0f, 0f, -5.5f)));
        }

        [Test]
        public void RandomPoint_IsAlwaysInsideTheViewKeepingTheInset()
        {
            for (int i = 0; i < 200; i++)
            {
                Vector3 point = playfield.RandomPoint(1f);

                Assert.LessOrEqual(Mathf.Abs(point.x), 9f + 1e-3f);
                Assert.LessOrEqual(Mathf.Abs(point.z), 4f + 1e-3f);
                Assert.AreEqual(0f, point.y);
            }
        }
    }
}
