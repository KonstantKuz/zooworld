using System;
using System.Collections.Generic;
using Core.Actors.Abstract;
using Core.Actors.Modules.Abstract;
using Core.Pooling.Abstract;
using Game.Actors.Status;
using Game.Actors.Status.Abstract;
using R3;
using UnityEngine;
using Zenject;

namespace UI
{
    public sealed class StatusLabelsView : MonoBehaviour
    {
        private readonly struct Entry
        {
            public Entry(ActorLabel label, IDisposable follow)
            {
                Label = label;
                Follow = follow;
            }

            public ActorLabel Label { get; }
            public IDisposable Follow { get; }
        }

        [SerializeField] private ActorLabel labelPrefab;
        [SerializeField, Min(0)] private int prewarm = 6;
        [Tooltip("Where the label floats relative to the actor, in world units.")]
        [SerializeField] private Vector3 worldOffset = new(0f, 0f, 1.2f);

        private readonly Dictionary<(int Id, Type Type), Entry> entries = new();
        private Camera worldCamera;
        private IPoolService pools;
        private IActorStatus statuses;

        [Inject]
        private void Construct(IActorStatus statuses, IPoolService pools, Camera camera)
        {
            this.statuses = statuses;
            this.pools = pools;
            worldCamera = camera;
        }

        private void Start()
        {
            pools.Prewarm(labelPrefab, prewarm);
            statuses.Changed.Subscribe(OnChanged).AddTo(this);
        }

        private void OnDestroy()
        {
            foreach (Entry entry in entries.Values)
            {
                entry.Follow.Dispose();
            }
        }

        private void OnChanged(ActorStatusChanged change)
        {
            (int, Type) key = (change.Actor.Id, change.Status.GetType());
            if (change.Active)
            {
                Show(key, change);
            }
            else
            {
                Hide(key);
            }
        }

        private void Show((int Id, Type Type) key, ActorStatusChanged change)
        {
            if (entries.ContainsKey(key))
            {
                return;
            }

            ActorLabel label = pools.Get(labelPrefab, Vector3.zero, Quaternion.identity, transform);
            label.SetText(change.Status.Message, change.Status.Color);
            Place(label, change.Actor);
            IDisposable follow = Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate)
                .Subscribe(_ => Place(label, change.Actor));
            entries.Add(key, new Entry(label, follow));
        }

        private void Hide((int Id, Type Type) key)
        {
            if (!entries.Remove(key, out Entry entry))
            {
                return;
            }

            entry.Follow.Dispose();
            pools.Release(entry.Label);
        }

        private void Place(ActorLabel label, IActor actor)
        {
            if (actor.TryGetPosition(out Vector3 position))
            {
                label.SetScreenPosition(worldCamera.WorldToScreenPoint(position + worldOffset));
            }
        }
    }
}
