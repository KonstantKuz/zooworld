using Core.Actors.Abstract;
using Core.Actors.Interactions;
using Core.Actors;
using Zenject;

namespace Core.Installers
{
    public static class ActorsInstaller
    {
        public static void Install(DiContainer container)
        {
            container.BindInterfacesTo<CollisionService>().AsSingle();
            container.Bind<IActorFactory>().To<ActorFactory>().AsSingle();
            container.BindInterfacesTo<ActorLifecycleService>().AsSingle();
        }
    }
}
