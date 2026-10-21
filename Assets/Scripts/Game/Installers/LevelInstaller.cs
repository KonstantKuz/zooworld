using Game.Level.Configs;
using Game.Level;
using Zenject;

namespace Game.Installers
{
    public static class LevelInstaller
    {
        public static void Install(DiContainer container, LevelSettings settings, Obstacle obstaclePrefab)
        {
            container.BindInstance(settings);
            container.BindInstance(obstaclePrefab);
            container.BindInterfacesTo<LevelGenerationService>().AsSingle();

            container.BindInitializableExecutionOrder<LevelGenerationService>(-100);
        }
    }
}
