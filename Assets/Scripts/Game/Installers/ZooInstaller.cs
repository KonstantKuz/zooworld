using Core.Actors.Configs;
using Core.Installers;
using Core.Pooling.Configs;
using Game.Level.Configs;
using Game.Level;
using Game.Navigation.Configs;
using Game.Spawning;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    public sealed class ZooInstaller : MonoInstaller
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private ActorCatalog catalog;
        [SerializeField] private Obstacle obstaclePrefab;
        [SerializeField] private SpawnSettings spawnSettings;
        [SerializeField] private LevelSettings levelSettings;
        [SerializeField] private NavigationSettings navigationSettings;
        [SerializeField] private PoolSettings poolSettings;
        [SerializeField, Min(0f)] private float preyBounceSpeed = 3f;

        public override void InstallBindings()
        {
            Container.BindInstance(worldCamera);
            Container.BindInstance(catalog);

            PoolingInstaller.Install(Container, poolSettings);
            NavigationInstaller.Install(Container, navigationSettings);
            ActorsInstaller.Install(Container);
            LevelInstaller.Install(Container, levelSettings, obstaclePrefab);
            AnimalsInstaller.Install(Container, spawnSettings, preyBounceSpeed);
        }
    }
}
