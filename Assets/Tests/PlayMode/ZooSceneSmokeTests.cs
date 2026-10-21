using System.Collections;
using Core.Actors;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    /// <summary>The whole game wired by Zenject: scene, installers, pools, NavMesh, spawning, behaviours.</summary>
    public sealed class ZooSceneSmokeTests
    {
        private const string SceneName = "Zoo";

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator TheZooComesAliveAndRunsWithoutErrors()
        {
            yield return new WaitForSeconds(8f);

            Actor[] animals = Object.FindObjectsByType<Actor>(FindObjectsSortMode.None);

            Assert.Greater(animals.Length, 0, "animals should have spawned by now");
            foreach (Actor animal in animals)
            {
                Assert.Greater(animal.transform.position.y, -5f, $"{animal.name} fell through the floor");
            }
        }
    }
}
