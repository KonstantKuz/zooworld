using Core.Pooling.Abstract;
using Core.Pooling.Configs;
using Core.Pooling;
using Zenject;

namespace Core.Installers
{
    public static class PoolingInstaller
    {
        public static void Install(DiContainer container, PoolSettings settings)
        {
            container.BindInstance(settings);
            container.Bind<IPoolService>().To<PoolService>().AsSingle();
        }
    }
}
