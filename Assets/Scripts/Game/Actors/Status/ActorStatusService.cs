using System;
using System.Collections.Generic;
using Core.Actors.Abstract;
using Game.Actors.Status.Abstract;
using R3;
using Zenject;

namespace Game.Actors.Status
{
    public sealed class ActorStatusService : IActorStatus, IInitializable, IDisposable
    {
        private readonly IActorEvents events;
        private readonly Subject<ActorStatusChanged> changed = new();
        private readonly Dictionary<Type, ActorStatus> statuses = new();
        private readonly Dictionary<(int Id, Type Type), ActorStatusChanged> active = new();
        private readonly List<ActorStatusChanged> buffer = new();
        private IDisposable subscription;

        public ActorStatusService(IActorEvents events)
        {
            this.events = events;
        }

        public Observable<ActorStatusChanged> Changed => changed;

        public void Initialize()
        {
            subscription = events.Died.Subscribe(death => Clear(death.Victim));
        }

        public void Dispose()
        {
            subscription?.Dispose();
            changed.Dispose();
        }

        public void Set<T>(IActor actor, bool isActive) where T : ActorStatus, new()
        {
            if (!statuses.TryGetValue(typeof(T), out ActorStatus status))
            {
                status = new T();
                statuses[typeof(T)] = status;
            }

            Apply(actor, status, isActive);
        }

        private void Apply(IActor actor, ActorStatus status, bool isActive)
        {
            (int, Type) key = (actor.Id, status.GetType());
            if (isActive == active.ContainsKey(key))
            {
                return;
            }

            ActorStatusChanged change = new ActorStatusChanged(actor, status, isActive);
            if (isActive)
            {
                active.Add(key, change);
            }
            else
            {
                active.Remove(key);
            }

            changed.OnNext(change);
        }

        private void Clear(IActor actor)
        {
            buffer.Clear();
            foreach (KeyValuePair<(int Id, Type Type), ActorStatusChanged> entry in active)
            {
                if (entry.Key.Id == actor.Id)
                {
                    buffer.Add(entry.Value);
                }
            }

            foreach (ActorStatusChanged change in buffer)
            {
                Apply(actor, change.Status, false);
            }
        }
    }
}
