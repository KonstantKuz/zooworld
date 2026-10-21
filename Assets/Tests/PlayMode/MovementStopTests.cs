using System.Collections;
using Game.Actors.Modules.Movement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public sealed class MovementStopTests
    {
        private sealed class ConstantRoute : Core.Actors.Modules.Movement.Abstract.IRoute
        {
            public Vector3 NextDirection(Vector3 position, float deltaTime)
            {
                return Vector3.right;
            }
        }

        private GameObject floor;
        private GameObject animal;
        private Rigidbody body;

        [SetUp]
        public void SetUp()
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(100f, 1f, 100f);

            animal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            animal.transform.position = new Vector3(0f, 0.5f, 0f);
            body = animal.AddComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(floor);
            Object.Destroy(animal);
        }

        [UnityTest]
        public IEnumerator LinearMovement_StopBringsARunningAnimalToAHalt()
        {
            LinearMovement movement = new(body, new VelocityFacing(body, 10f), 4f, 30f);

            for (int i = 0; i < 50; i++)
            {
                movement.Tick(new ConstantRoute(), Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            Assert.Greater(PlanarSpeed(), 3f, "it was running");

            for (int i = 0; i < 50; i++)
            {
                movement.Stop(Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            Assert.Less(PlanarSpeed(), 0.1f);
        }

        [UnityTest]
        public IEnumerator JumpMovement_StopKillsTheGroundSpeedOfALandedAnimal()
        {
            JumpMovement movement = new(body, new VelocityFacing(body, 10f), 1f, 0.6f, 6f, 5f, 3f, 0f);
            body.linearVelocity = new Vector3(3f, 0f, 0f);
            yield return new WaitForFixedUpdate();

            for (int i = 0; i < 50; i++)
            {
                movement.Stop(Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            Assert.Less(PlanarSpeed(), 0.2f);
        }

        private float PlanarSpeed()
        {
            Vector3 velocity = body.linearVelocity;
            return new Vector2(velocity.x, velocity.z).magnitude;
        }
    }
}
