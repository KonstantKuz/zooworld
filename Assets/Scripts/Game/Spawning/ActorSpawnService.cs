using System.Threading;
using System;
using Core.Actors.Abstract;
using Core.Actors.Configs;
using Core.Navigation.Abstract;
using Core.Pooling.Abstract;
using Core.Pooling.Configs;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;
using UnityEngine;
using Zenject;

namespace Game.Spawning
{
    public sealed class ActorSpawnService : IInitializable, IDisposable
    {
        private const int FreePointAttempts = 8;
        private const float FloorGap = 0.1f;

        private readonly ActorCatalog catalog;
        private readonly SpawnSettings settings;
        private readonly IActorLifecycle lifecycle;
        private readonly INavigationService navigation;
        private readonly IPoolService pools;
        private readonly PoolSettings poolSettings;
        private readonly CancellationTokenSource lifetime = new();

        public ActorSpawnService(ActorCatalog catalog, SpawnSettings settings, IActorLifecycle lifecycle,
            INavigationService navigation, IPoolService pools, PoolSettings poolSettings)
        {
            this.catalog = catalog;
            this.settings = settings;
            this.lifecycle = lifecycle;
            this.navigation = navigation;
            this.pools = pools;
            this.poolSettings = poolSettings;
        }

        public void Initialize()
        {
            for (int i = 0; i < catalog.Actors.Count; i++)
            {
                pools.Prewarm(catalog.Actors[i].Prefab, poolSettings.InitialCapacity);
            }

            SpawnLoopAsync(lifetime.Token).Forget();
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }

        private async UniTaskVoid SpawnLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                TimeSpan delay = TimeSpan.FromSeconds(Random.Range(settings.MinInterval, settings.MaxInterval));
                if (await UniTask.Delay(delay, cancellationToken: token).SuppressCancellationThrow())
                {
                    return;
                }

                SpawnOne();
            }
        }

        private void SpawnOne()
        {
            if (lifecycle.ActiveCount >= settings.MaxAnimals || !TryFindFreePoint(out Vector3 point))
            {
                return;
            }

            ActorConfig config = catalog.GetRandom();
            lifecycle.Spawn(config, point + Vector3.up * config.SpawnHeight);
        }

        private bool TryFindFreePoint(out Vector3 point)
        {
            for (int i = 0; i < FreePointAttempts; i++)
            {
                if (navigation.TryGetRandomPoint(out point) && IsFree(point))
                {
                    return true;
                }
            }

            point = default;
            return false;
        }

        private bool IsFree(Vector3 point)
        {
            float radius = settings.FreeSpaceRadius;
            return !Physics.CheckSphere(point + Vector3.up * (radius + FloorGap), radius);
        }
    }
}
