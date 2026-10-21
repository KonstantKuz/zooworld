using Core.Actors.Interactions.Abstract;
using Game.Actors.Interactions;
using Game.Actors.Status;
using Game.Spawning;
using Game.Stats;
using Zenject;

namespace Game.Installers
{
    public static class AnimalsInstaller
    {
        public static void Install(DiContainer container, SpawnSettings spawnSettings, float preyBounceSpeed)
        {
            container.BindInstance(spawnSettings);

            container.Bind<IInteractionRule>().To<PredatorEatsPreyRule>().AsSingle();
            container.Bind<IInteractionRule>().To<PredatorDuelRule>().AsSingle();
            container.Bind<IInteractionRule>().To<PreyBounceRule>().AsSingle().WithArguments(preyBounceSpeed);

            container.BindInterfacesTo<ActorSpawnService>().AsSingle();
            container.BindInterfacesAndSelfTo<KillStatsService>().AsSingle();
            container.BindInterfacesAndSelfTo<ActorStatusService>().AsSingle();
        }
    }
}
