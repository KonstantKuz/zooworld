using System.Collections.Generic;
using System;
using Core.Actors.Abstract;
using Core.Actors.Modules.Abstract;
using Core.Pooling.Abstract;
using Game.Actors.Traits.Abstract;
using R3;
using UnityEngine;
using Zenject;

namespace UI
{
    public sealed class TastyView : MonoBehaviour
    {
        private const string Message = "Tasty!";

        [SerializeField] private ActorLabel labelPrefab;
        [SerializeField, Min(0)] private int prewarm = 3;

        private readonly HashSet<IDisposable> active = new();
        private Camera worldCamera;
        private IPoolService pools;
        private IActorEvents events;

        [Inject]
        private void Construct(IActorEvents events, IPoolService pools, Camera camera)
        {
            this.events = events;
            this.pools = pools;
            worldCamera = camera;
        }

        private void Start()
        {
            pools.Prewarm(labelPrefab, prewarm);

            events.Died
                .Where(death => death.Killer != null && death.Killer.HasTrait<IPredator>())
                .Subscribe(death => Show(death.Killer))
                .AddTo(this);
        }

        private void OnDestroy()
        {
            foreach (IDisposable follow in active)
            {
                follow.Dispose();
            }
        }

        private void Show(IActor predator)
        {
            ActorLabel label = pools.Get(labelPrefab, Vector3.zero, Quaternion.identity, transform);
            label.SetText(Message, Color.yellow);
            Place(label, predator);

            IDisposable follow = null;
            follow = Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate)
                .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(label.Duration)))
                .Subscribe(_ => Place(label, predator), _ =>
                {
                    active.Remove(follow);
                    pools.Release(label);
                });
            active.Add(follow);
        }

        private void Place(ActorLabel label, IActor predator)
        {
            if (predator.TryGetPosition(out Vector3 position))
            {
                label.SetScreenPosition(worldCamera.WorldToScreenPoint(position + label.WorldOffset));
            }
        }
    }
}
