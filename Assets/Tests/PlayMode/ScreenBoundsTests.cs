using Game.Actors.Modules.Movement;
using Game.Navigation;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    /// <summary>Brief: "If animals move out of screen it changed movement direction to return back to screen".</summary>
    public sealed class ScreenBoundsTests
    {
        private GameObject cameraObject;
        private CameraPlayfieldService playfield;
        private FakeNavigation navigation;

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
            // Open ground: a wanderer would pick its own targets far to the right, outside of the screen.
            navigation = new FakeNavigation { RandomPoint = new Vector3(40f, 0f, 0f) };
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(cameraObject);
        }

        [Test]
        public void AnAnimalThatLeftTheScreen_TurnsBackTowardIt()
        {
            WanderRoute route = new(navigation, playfield);

            Vector3 direction = route.NextDirection(new Vector3(30f, 0f, 0f), 0.02f);

            Assert.Less(direction.x, -0.5f, "heads back to the screen instead of on to its far target");
        }

        [Test]
        public void AnAnimalThatKeepsGoingOutside_EventuallyWalksBackOntoTheScreen()
        {
            WanderRoute route = new(navigation, playfield);
            Vector3 position = new(-25f, 0f, 12f);

            int steps = 0;
            while (!playfield.Contains(position) && steps++ < 500)
            {
                position += route.NextDirection(position, 0.02f) * 0.5f;
            }

            Assert.IsTrue(playfield.Contains(position), $"still outside after {steps} steps at {position}");
        }

        [Test]
        public void AnAnimalOnTheScreen_PursuesItsOwnTarget()
        {
            WanderRoute route = new(navigation, playfield);

            Vector3 direction = route.NextDirection(new Vector3(2f, 0f, 0f), 0.02f);

            Assert.Greater(direction.x, 0.5f);
        }
    }
}
