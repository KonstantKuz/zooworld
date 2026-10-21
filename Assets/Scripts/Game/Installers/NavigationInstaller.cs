using Core.Navigation.Abstract;
using Game.Navigation.Configs;
using Game.Navigation;
using Zenject;

namespace Game.Installers
{
    public static class NavigationInstaller
    {
        public static void Install(DiContainer container, NavigationSettings settings)
        {
            container.BindInstance(settings);
            container.Bind<IPlayfieldService>().To<CameraPlayfieldService>().AsSingle();
            container.BindInterfacesTo<NavMeshNavigationService>().AsSingle();
        }
    }
}
